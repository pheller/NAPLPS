// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using NAPLPS.Telidraw;

namespace NAPLPSTests.Telidraw;

/// <summary>
/// The headline regression test from the Phase 8 plan:
///   For every file in Examples/: load .nap → decompile → recompile → byte-equal to the
///   RAW file bytes. Comparing against the raw file (not original.ToBytes()) makes this
///   end-to-end: parser serialization lossiness cannot hide behind an equally lossy
///   reference.
/// </summary>
[TestClass]
public class ExamplesRoundTripTests
{
    private static string ExamplesDir => Path.Combine(AppContext.BaseDirectory, "examples");

    private static readonly string[] SkipExtensions = [".jpg", ".png", ".txt", ".exe"];

    [TestMethod]
    [TestCategory("RoundTrip")]
    public void DecompileRecompile_AllExamples()
    {
        if (!Directory.Exists(ExamplesDir))
        {
            Assert.Inconclusive($"Examples directory not found: {ExamplesDir}");
            return;
        }

        var files = Directory.GetFiles(ExamplesDir, "*", SearchOption.AllDirectories)
            .Where(f => !SkipExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .OrderBy(f => f)
            .ToList();

        int passed = 0;
        int failed = 0;
        int errored = 0;
        var failures = new List<string>();

        foreach (var file in files)
        {
            var relative = Path.GetRelativePath(ExamplesDir, file);

            try
            {
                // Load the original .nap
                var original = NaplpsFormat.FromFile(file);

                if (original.IsErrored)
                {
                    // Skip files that don't load cleanly — they have parse errors
                    // independent of the decompiler.
                    errored++;
                    continue;
                }

                var originalBytes = System.IO.File.ReadAllBytes(file);

                // Decompile to .td source
                var tdSource = Decompiler.Decompile(original);

                // Recompile from .td
                var tokens = new Lexer(tdSource).Tokenize();
                var parser = new Parser(tokens);
                var ast = parser.Parse();

                if (parser.Diagnostics.Count > 0)
                {
                    failures.Add($"{relative}: {parser.Diagnostics.Count} parse error(s) in decompiled source");
                    failed++;
                    continue;
                }

                var compiler = new Compiler(ast) { BareFormat = true };
                var recompiled = compiler.Compile();

                if (compiler.Diagnostics.Count > 0)
                {
                    failures.Add($"{relative}: {compiler.Diagnostics.Count} compile error(s) from decompiled source");
                    failed++;
                    continue;
                }

                var recompiledBytes = recompiled.ToBytes();

                if (originalBytes.SequenceEqual(recompiledBytes))
                {
                    passed++;
                }
                else
                {
                    var diff = Math.Abs(originalBytes.Length - recompiledBytes.Length);
                    failures.Add($"{relative}: byte mismatch (original={originalBytes.Length}B, recompiled={recompiledBytes.Length}B, delta={diff}B)");
                    failed++;
                }
            }
            catch (Exception ex)
            {
                failures.Add($"{relative}: EXCEPTION {ex.GetType().Name}: {ex.Message}");
                errored++;
            }
        }

        // Report — write to file since Console doesn't show in quiet mode
        var report = new System.Text.StringBuilder();
        report.AppendLine($"=== Telidraw Round-Trip Report ===");
        report.AppendLine($"Total files: {files.Count}");
        report.AppendLine($"Byte-identical: {passed}");
        report.AppendLine($"Mismatched: {failed}");
        report.AppendLine($"Errored/skipped: {errored}");
        report.AppendLine($"Coverage: {(files.Count > 0 ? 100.0 * passed / files.Count : 0):F1}%");

        if (failures.Count > 0)
        {
            report.AppendLine("--- Failures ---");
            foreach (var f in failures.Take(50))
            {
                report.AppendLine($"  {f}");
            }

            if (failures.Count > 50)
            {
                report.AppendLine($"  ... and {failures.Count - 50} more");
            }
        }

        var reportPath = Path.Combine(AppContext.BaseDirectory, "td_roundtrip_report.txt");
        System.IO.File.WriteAllText(reportPath, report.ToString());

        Assert.AreEqual(0, failed + errored, $"{failed} mismatch + {errored} errors. Report: {reportPath}");
    }
}
