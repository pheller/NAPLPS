// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.File;

[TestClass]
public class AllExamplesLoadTest
{
    private static readonly string[] SkipExtensions = [".jpg", ".png", ".txt", ".exe"];

    private static string ExamplesDir => Path.Combine(AppContext.BaseDirectory, "examples");

    public static IEnumerable<object[]> AllExampleFiles
    {
        get
        {
            var files = Directory.GetFiles(ExamplesDir, "*", SearchOption.AllDirectories)
                .Where(f => !SkipExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .OrderBy(f => f);

            foreach (var file in files)
            {
                yield return [Path.GetRelativePath(ExamplesDir, file)];
            }
        }
    }

    [TestMethod]
    [DynamicData(nameof(AllExampleFiles))]
    public void LoadWithoutCrashOrError(string relativePath)
    {
        var fullPath = Path.Combine(ExamplesDir, relativePath);

        var file = NaplpsFormat.FromFile(fullPath);

        Assert.IsNotNull(file, $"FromFile returned null for {relativePath}");
        Assert.IsFalse(file.IsErrored, $"{relativePath} had errors: {string.Join("; ", file.Errors.Where(e => e.Severity == NaplpsErrorSeverity.Error))}");
    }
}
