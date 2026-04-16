// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using NAPLPS.Telidraw;

namespace NAPLPSTests.Telidraw;

/// <summary>
/// Headline round-trip tests: compile a Telidraw program, decompile the resulting
/// NaplpsFormat back to source text, recompile that source, and verify the two byte
/// streams are identical. This is the contract that makes .td a lossless human-editable
/// save format for compiler-produced .nap files.
/// </summary>
[TestClass]
public class DecompilerTests
{
    /// <summary>
    /// Compile source → decompile → recompile → byte-equal to first compile.
    /// </summary>
    private static void AssertRoundTrip(string source)
    {
        // First compile
        var tokens1 = new Lexer(source).Tokenize();
        var parser1 = new Parser(tokens1);
        var ast1 = parser1.Parse();
        Assert.AreEqual(0, parser1.Diagnostics.Count, $"Parse errors on first compile: {string.Join("; ", parser1.Diagnostics)}");
        var compiler1 = new Compiler(ast1);
        var format1 = compiler1.Compile();
        Assert.AreEqual(0, compiler1.Diagnostics.Count, $"Compile errors on first compile: {string.Join("; ", compiler1.Diagnostics)}");
        var bytes1 = format1.ToBytes();

        // Decompile
        var decompiled = Decompiler.Decompile(format1);

        // Recompile from decompiled source
        var tokens2 = new Lexer(decompiled).Tokenize();
        var parser2 = new Parser(tokens2);
        var ast2 = parser2.Parse();
        Assert.AreEqual(0, parser2.Diagnostics.Count, $"Parse errors on recompile:\n{decompiled}\n---\n{string.Join("\n", parser2.Diagnostics)}");
        var compiler2 = new Compiler(ast2) { BareFormat = true };
        var format2 = compiler2.Compile();
        Assert.AreEqual(0, compiler2.Diagnostics.Count, $"Compile errors on recompile:\n{decompiled}\n---\n{string.Join("\n", compiler2.Diagnostics)}");
        var bytes2 = format2.ToBytes();

        // Byte-equal
        CollectionAssert.AreEqual(bytes1, bytes2, $"Byte mismatch after decompile→recompile.\nOriginal source:\n{source}\n\nDecompiled:\n{decompiled}");
    }

    [TestMethod]
    public void RoundTrip_MoveAndLine()
    {
        AssertRoundTrip("move 0.5 0.5\nline 0.75 0.25");
    }

    [TestMethod]
    public void RoundTrip_RectFilled()
    {
        AssertRoundTrip("move 0.125 0.125\nrect 0.25 0.25");
    }

    [TestMethod]
    public void RoundTrip_ColorAndRect()
    {
        AssertRoundTrip("color 3\nmove 0.125 0.125\nrect 0.25 0.25");
    }

    [TestMethod]
    public void RoundTrip_WithColorBlock()
    {
        // With-block emits color 5, body, color 7 (restore). Decompiler sees flat commands.
        AssertRoundTrip("with color 5 { move 0.25 0.25 rect 0.125 0.125 }");
    }

    [TestMethod]
    public void RoundTrip_RepeatUnrolled()
    {
        AssertRoundTrip("repeat 3 { color 2 }");
    }

    [TestMethod]
    public void RoundTrip_ForLoop()
    {
        AssertRoundTrip("for i in 1..4 { color i }");
    }

    [TestMethod]
    public void RoundTrip_Wait()
    {
        AssertRoundTrip("wait 15");
    }

    [TestMethod]
    public void RoundTrip_Reset()
    {
        AssertRoundTrip("reset");
    }

    [TestMethod]
    public void RoundTrip_Field()
    {
        AssertRoundTrip("field");
    }

    [TestMethod]
    public void RoundTrip_ProcInline()
    {
        AssertRoundTrip("""
            proc box(x, y, s) {
              move x y
              rect s s
            }
            box 0.125 0.125 0.25
            """);
    }

    [TestMethod]
    public void RoundTrip_PaletteAlias()
    {
        AssertRoundTrip("palette red = 4\ncolor red");
    }

    [TestMethod]
    public void RoundTrip_Forward()
    {
        // Turtle at heading 0 (→ +X): forward 0.25 from (0.125, 0.125) lands at (0.375, 0.125).
        // Decompiler sees LineAbsolute(0.375, 0.125) and emits `line 0.375 0.125`.
        // Recompile of `line 0.375 0.125` produces the same bytes.
        AssertRoundTrip("move 0.125 0.125\nforward 0.25");
    }

    [TestMethod]
    public void Decompile_ProducesValidTelidrawSource()
    {
        // Just verify the decompiled text can parse without errors.
        var source = "color 7\nmove 0.5 0.5\nrect 0.25 0.25\nwait 10\nreset";
        var format = CompileSource(source);
        var decompiled = Decompiler.Decompile(format);

        var tokens = new Lexer(decompiled).Tokenize();
        var parser = new Parser(tokens);
        parser.Parse();
        Assert.AreEqual(0, parser.Diagnostics.Count, $"Decompiled source has parse errors:\n{decompiled}\n{string.Join("\n", parser.Diagnostics)}");
    }

    [TestMethod]
    public void Decompile_SkipsFormatNewSentinels()
    {
        var format = NaplpsFormat.New();
        var decompiled = Decompiler.Decompile(format);

        // Should not contain `cancel` or `nonSelectiveReset` in the output.
        Assert.IsFalse(decompiled.Contains("cancel", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(decompiled.Contains("nonSelectiveReset", StringComparison.OrdinalIgnoreCase));
    }

    private static NaplpsFormat CompileSource(string source)
    {
        var tokens = new Lexer(source).Tokenize();
        var parser = new Parser(tokens);
        var ast = parser.Parse();
        var compiler = new Compiler(ast);
        return compiler.Compile();
    }
}
