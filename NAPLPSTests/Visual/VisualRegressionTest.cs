// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NAPLPSTests.Visual;

[TestClass]
public class VisualRegressionTest
{
    [TestMethod]
    [TestCategory("VR")]
    public void VisualBaselines()
    {
        VisualTestContext.CleanOutputDirs();

        var files = VisualTestContext.DiscoverExampleFiles().ToList();
        var failures = new System.Collections.Concurrent.ConcurrentBag<string>();

        Parallel.ForEach(files, relativePath =>
        {
            ProcessFile(relativePath, failures);
        });

        VisualTestContext.GenerateReport(VisualTestContext.Results);

        if (failures.Count > 0)
        {
            Assert.Fail($"{failures.Count} visual regression(s) detected. See report: {VisualTestContext.ReportPath}");
        }

        var newCount = VisualTestContext.Results.Values.Count(r => r.Status == VisualTestStatus.New);

        if (newCount > 0)
        {
            Assert.Inconclusive($"{newCount} new baseline(s) need to be accepted. See report: {VisualTestContext.ReportPath}");
        }
    }

    private static void ProcessFile(string relativePath, System.Collections.Concurrent.ConcurrentBag<string> failures)
    {
        var fullPath = Path.Combine(VisualTestContext.ExamplesDir, relativePath);
        var baselinePath = VisualTestContext.GetBaselinePath(relativePath);
        var actualPath = VisualTestContext.GetActualPath(relativePath);

        Image<Rgba32>? apng = null;

        try
        {
            apng = VisualTestContext.RenderApng(fullPath);
        }
        catch (Exception ex)
        {
            VisualTestContext.Results[relativePath] = new VisualTestResult(relativePath, VisualTestStatus.Error, baselinePath, null, null, 0, 0, 0, ex.Message);
            return;
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(actualPath)!);
            apng.SaveAsPng(actualPath);
            var frameCount = apng.Frames.Count;
            apng.Dispose();

            var diffHtmlPath = VisualTestContext.GetDiffHtmlPath(relativePath);

            if (!System.IO.File.Exists(baselinePath))
            {
                VisualTestContext.GenerateViewHtml(relativePath, actualPath, frameCount, diffHtmlPath);
                VisualTestContext.Results[relativePath] = new VisualTestResult(relativePath, VisualTestStatus.New, null, actualPath, diffHtmlPath, frameCount, 0, 0, null);
                return;
            }

            var comparison = VisualTestContext.CompareApngs(baselinePath, actualPath);

            if (comparison.AreIdentical)
            {
                VisualTestContext.GenerateDiffHtml(relativePath, comparison, baselinePath, actualPath, diffHtmlPath);
                VisualTestContext.Results[relativePath] = new VisualTestResult(relativePath, VisualTestStatus.Pass, baselinePath, actualPath, diffHtmlPath, frameCount, 0, 0, null);
                return;
            }

            VisualTestContext.GenerateDiffHtml(relativePath, comparison, baselinePath, actualPath, diffHtmlPath);

            foreach (var fd in comparison.FrameDiffs)
            {
                fd.DiffImage?.Dispose();
            }

            VisualTestContext.Results[relativePath] = new VisualTestResult(relativePath, VisualTestStatus.Fail, baselinePath, actualPath, diffHtmlPath, frameCount, comparison.FrameDiffs.Count(f => f.DiffPixelCount > 0), comparison.TotalDiffPixels, null);
            failures.Add(relativePath);
        }
        catch (Exception ex)
        {
            VisualTestContext.Results[relativePath] = new VisualTestResult(relativePath, VisualTestStatus.Error, baselinePath, actualPath, null, 0, 0, 0, ex.Message);
        }
    }
}
