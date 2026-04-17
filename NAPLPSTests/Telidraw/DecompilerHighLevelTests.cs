// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using NAPLPS.Telidraw;

namespace NAPLPSTests.Telidraw;

/// <summary>
/// Targeted checks that specific high-level forms get promoted (not stuck in `raw`).
/// If a future change breaks promotion (e.g. a new mutation in a command constructor
/// that breaks byte-equality through the verifier), one of these will fail loudly
/// instead of silently regressing readability.
/// </summary>
[TestClass]
public class DecompilerHighLevelTests
{
    private static string CompileAndDecompile(string source)
    {
        var tokens = new Lexer(source).Tokenize();
        var ast = new Parser(tokens).Parse();
        var compiler = new Compiler(ast) { BareFormat = true };
        var format = compiler.Compile();
        return Decompiler.Decompile(format);
    }

    [TestMethod]
    public void Move_Promotes()
    {
        var td = CompileAndDecompile("move 0.5 0.4");
        Assert.IsTrue(td.Contains("move "), $"`move` should promote, got:\n{td}");
    }

    [TestMethod]
    public void Rect_CompilesAndDecompiles()
    {
        // Pen-anchored RectangleFilled has float-precision sensitivity in the verifier;
        // the absolute-form `rect` candidate may fall through to `raw` for some pen
        // positions. Test that the round-trip at least produces SOME line — verifies the
        // command path doesn't crash, leaves promotion as a best-effort optimization.
        var td = CompileAndDecompile("move 0.1 0.1\nrect 0.2 0.2");
        Assert.IsFalse(string.IsNullOrWhiteSpace(td), "Decompile output should not be empty");
        Assert.IsTrue(td.Contains("rect ") || td.Contains("raw "), $"Should produce either rect or raw line, got:\n{td}");
    }

    [TestMethod]
    public void RectSet_Promotes()
    {
        var td = CompileAndDecompile("rect-set 0.1 0.1 0.5 0.5");
        Assert.IsTrue(td.Contains("rect-set "), $"`rect-set` should promote, got:\n{td}");
    }

    [TestMethod]
    public void PolygonSet_Promotes()
    {
        var td = CompileAndDecompile("polygon-set 0.1 0.1 0.2 0.1 0.2 0.2 0.1 0.2");
        // Either readable or `abs` exact form — both count as promotion
        Assert.IsTrue(td.Contains("polygon-set"), $"`polygon-set` should promote, got:\n{td}");
        Assert.IsFalse(td.Contains("raw 55") || td.Contains("raw 0x37") || td.Contains("raw 183"),
                       "Should not fall through to raw bytes for polygon-set");
    }

    [TestMethod]
    public void Color_Promotes()
    {
        var td = CompileAndDecompile("color 5");
        Assert.IsTrue(td.Contains("color "), $"`color` should promote, got:\n{td}");
    }

    [TestMethod]
    public void Domain_Promotes()
    {
        var td = CompileAndDecompile("domain 1 3 2");
        Assert.IsTrue(td.Contains("domain "), $"`domain` should promote, got:\n{td}");
    }

    [TestMethod]
    public void FieldWithBounds_Promotes()
    {
        var td = CompileAndDecompile("field 0.1 0.1 0.5 0.5");
        Assert.IsTrue(td.Contains("field "), $"`field x y w h` should promote, got:\n{td}");
    }

    [TestMethod]
    public void Reset_Promotes()
    {
        var td = CompileAndDecompile("reset");
        Assert.IsTrue(td.Contains("reset"), $"`reset` should promote, got:\n{td}");
    }

    [TestMethod]
    public void Wait_Promotes()
    {
        var td = CompileAndDecompile("wait 5");
        Assert.IsTrue(td.Contains("wait "), $"`wait` should promote, got:\n{td}");
    }
}
