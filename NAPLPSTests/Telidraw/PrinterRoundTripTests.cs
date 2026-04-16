// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using NAPLPS.Telidraw;

namespace NAPLPSTests.Telidraw;

/// <summary>
/// Verifies the Lexer \u2192 Parser \u2192 AstPrinter \u2192 Lexer \u2192 Parser roundtrip produces an
/// equivalent AST structure. The printed form may differ in whitespace from the original
/// source, but re-parsing it must yield a program with the same shape.
/// </summary>
[TestClass]
public class PrinterRoundTripTests
{
    private static ProgramNode ParseOrFail(string src)
    {
        var tokens = new Lexer(src).Tokenize();
        var parser = new Parser(tokens);
        var program = parser.Parse();
        Assert.AreEqual(0, parser.Diagnostics.Count, $"Parse errors: {string.Join("; ", parser.Diagnostics)}");
        return program;
    }

    private static void AssertRoundTrip(string src)
    {
        var first = ParseOrFail(src);
        var printed = AstPrinter.Print(first);
        var second = ParseOrFail(printed);

        // Shape comparison: same number of directives + statements, same leaf statement kinds.
        Assert.AreEqual(first.Directives.Count, second.Directives.Count, $"Directive count mismatch.\nFirst: {src}\nPrinted: {printed}");
        Assert.AreEqual(first.Statements.Count, second.Statements.Count, $"Statement count mismatch.\nFirst: {src}\nPrinted: {printed}");

        for (int i = 0; i < first.Statements.Count; i++)
        {
            Assert.AreEqual(first.Statements[i].GetType(), second.Statements[i].GetType(), $"Statement {i} type differs.\nFirst: {src}\nPrinted: {printed}");
        }
    }

    [TestMethod]
    public void RoundTrip_SimpleCommand()
    {
        AssertRoundTrip("move 0.5 0.5");
    }

    [TestMethod]
    public void RoundTrip_WithBlockAndRepeat()
    {
        AssertRoundTrip("with color 3 { repeat 5 { forward 10 turn 72 } }");
    }

    [TestMethod]
    public void RoundTrip_ProcDeclWithProcCall()
    {
        AssertRoundTrip("""
            proc star(x, y, size) {
              move x y
              repeat 5 {
                forward size
                turn 144
              }
            }

            with color 7 {
              star 0.5 0.5 0.1
            }
            """);
    }

    [TestMethod]
    public void RoundTrip_MacroProc()
    {
        AssertRoundTrip("@macro proc body() { forward 10 }");
    }

    [TestMethod]
    public void RoundTrip_DirectiveAndPaletteAliasAndLet()
    {
        AssertRoundTrip("""
            #coord pixels

            palette cyan = 1
            let step = 10

            with color cyan {
              forward step
            }
            """);
    }

    [TestMethod]
    public void RoundTrip_ForLoopAndIf()
    {
        AssertRoundTrip("""
            for i in 1..5 {
              forward i
              if i {
                turn 90
              } else {
                turn 45
              }
            }
            """);
    }

    [TestMethod]
    public void RoundTrip_FractionAndNegativeNumber()
    {
        AssertRoundTrip("domain 1/40 5/128\npoint -0.5 -0.25");
    }

    [TestMethod]
    public void RoundTrip_NestedWithBlocks()
    {
        AssertRoundTrip("with color 3 { with texture 1 { rect 0.1 0.1 } }");
    }
}
