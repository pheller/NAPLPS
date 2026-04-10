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

        Parallel.ForEach(files, relativePath =>
        {
            ProcessFile(relativePath);
        });

        VisualTestContext.GenerateReport(VisualTestContext.Results);
    }

    private static void ProcessFile(string relativePath)
    {
        var fullPath = Path.Combine(VisualTestContext.ExamplesDir, relativePath);
        var actualPath = VisualTestContext.GetActualPath(relativePath);

        Image<Rgba32>? apng = null;

        try
        {
            apng = VisualTestContext.RenderApng(fullPath);
        }
        catch (Exception ex)
        {
            VisualTestContext.Results[relativePath] = new VisualTestResult(relativePath, null, 0, ex.Message);
            return;
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(actualPath)!);
            apng.SaveAsPng(actualPath);
            var frameCount = apng.Frames.Count;
            apng.Dispose();

            var viewerPath = VisualTestContext.GetViewerHtmlPath(relativePath);
            VisualTestContext.GenerateViewerHtml(relativePath, actualPath, frameCount, viewerPath);

            VisualTestContext.Results[relativePath] = new VisualTestResult(relativePath, actualPath, frameCount, null);
        }
        catch (Exception ex)
        {
            VisualTestContext.Results[relativePath] = new VisualTestResult(relativePath, null, 0, ex.Message);
        }
    }
}
