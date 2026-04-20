// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using NAPLPS.Telidraw;

namespace NAPLPSTests.Telidraw;

[TestClass]
public class ParserTests
{
    private static ProgramNode Parse(string src)
    {
        var tokens = new Lexer(src).Tokenize();
        var parser = new Parser(tokens);
        var program = parser.Parse();
        Assert.AreEqual(0, parser.Diagnostics.Count, $"Unexpected parse errors: {string.Join("; ", parser.Diagnostics)}");
        return program;
    }

    [TestMethod]
    public void Parser_SimpleCommandCall_HasCorrectShape()
    {
        var program = Parse("move 0.5 0.5");

        Assert.AreEqual(1, program.Statements.Count);
        var cmd = (CommandCallNode)program.Statements[0];
        Assert.AreEqual(TokenKind.Move, cmd.Command);
        Assert.AreEqual(2, cmd.Args.Count);
    }

    [TestMethod]
    public void Parser_RepeatBlock_ContainsBody()
    {
        var program = Parse("repeat 5 { move 10 20 line 30 40 }");

        Assert.AreEqual(1, program.Statements.Count);
        var rep = (RepeatNode)program.Statements[0];
        Assert.IsInstanceOfType(rep.Count, typeof(NumberLiteralNode));
        Assert.AreEqual(2, rep.Body.Count);
    }

    [TestMethod]
    public void Parser_ForLoop_ParsesRange()
    {
        var program = Parse("for i in 1..10 { line i 0 }");

        var f = (ForNode)program.Statements[0];
        Assert.AreEqual("i", f.Variable);
        Assert.AreEqual(1.0, ((NumberLiteralNode)f.From).Value);
        Assert.AreEqual(10.0, ((NumberLiteralNode)f.To).Value);
        Assert.AreEqual(1, f.Body.Count);
    }

    [TestMethod]
    public void Parser_WithBlock_CapturesAttributeAndArgs()
    {
        var program = Parse("with color 3 { rect 0.1 0.1 }");

        var w = (WithBlockNode)program.Statements[0];
        Assert.AreEqual(TokenKind.Color, w.Attribute);
        Assert.AreEqual(1, w.AttributeArgs.Count);
        Assert.AreEqual(1, w.Body.Count);
    }

    [TestMethod]
    public void Parser_ProcDecl_CapturesParametersAndBody()
    {
        var program = Parse("proc star(x, y, size) { move x y line size size }");

        var p = (ProcDeclNode)program.Statements[0];
        Assert.AreEqual("star", p.Name);
        CollectionAssert.AreEqual(new[] { "x", "y", "size" }, p.Parameters.ToArray());
        Assert.AreEqual(2, p.Body.Count);
        Assert.IsFalse(p.AsMacro);
    }

    [TestMethod]
    public void Parser_MacroAttribute_MarksProcAsMacro()
    {
        var program = Parse("@macro proc m() { line 1 1 }");

        var p = (ProcDeclNode)program.Statements[0];
        Assert.IsTrue(p.AsMacro);
    }

    [TestMethod]
    public void Parser_Directive_HoistedToProgramHeader()
    {
        var program = Parse("#coord pixels\nline 10 20");

        Assert.AreEqual(1, program.Directives.Count);
        Assert.AreEqual("coord", program.Directives[0].Name);
        Assert.AreEqual(1, program.Statements.Count);
    }

    [TestMethod]
    public void Parser_PaletteAlias_RecordsNameAndValue()
    {
        var program = Parse("palette cyan = 1");

        var pa = (PaletteAliasNode)program.Statements[0];
        Assert.AreEqual("cyan", pa.Name);
        Assert.AreEqual(1.0, ((NumberLiteralNode)pa.Value).Value);
    }

    [TestMethod]
    public void Parser_ProcCall_DifferentiatedFromCommand()
    {
        var program = Parse("proc star(s) { } star 10");

        Assert.AreEqual(2, program.Statements.Count);
        Assert.IsInstanceOfType(program.Statements[0], typeof(ProcDeclNode));
        Assert.IsInstanceOfType(program.Statements[1], typeof(ProcCallNode));
    }

    [TestMethod]
    public void Parser_ExpressionPrecedence_IsStandard()
    {
        var program = Parse("wait 1 + 2 * 3");

        var cmd = (CommandCallNode)program.Statements[0];
        // 1 + (2 * 3)  — top is Plus with right being the * node
        var plus = (BinaryOpNode)cmd.Args[0];
        Assert.AreEqual("+", plus.Op);
        Assert.AreEqual(1.0, ((NumberLiteralNode)plus.Left).Value);
        var times = (BinaryOpNode)plus.Right;
        Assert.AreEqual("*", times.Op);
    }

    [TestMethod]
    public void Parser_Fraction_KeptAsFractionNode()
    {
        var program = Parse("domain 1/40 5/128");

        var cmd = (CommandCallNode)program.Statements[0];
        Assert.AreEqual(2, cmd.Args.Count);
        var f1 = (FractionLiteralNode)cmd.Args[0];
        Assert.AreEqual(1, f1.Numerator);
        Assert.AreEqual(40, f1.Denominator);
    }

    [TestMethod]
    public void Parser_ErrorRecovery_CollectsMultipleErrors()
    {
        var tokens = new Lexer("forward ?!@#$ back 5").Tokenize();
        var parser = new Parser(tokens);
        parser.Parse();

        Assert.IsTrue(parser.Diagnostics.Count > 0);
    }
}
