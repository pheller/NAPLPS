// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using NAPLPS.Telidraw;

namespace NAPLPSTests.Telidraw;

[TestClass]
public class LexerTests
{
    [TestMethod]
    public void Lexer_EmptyInput_ProducesOnlyEof()
    {
        var tokens = new Lexer(string.Empty).Tokenize();

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(TokenKind.Eof, tokens[0].Kind);
    }

    [TestMethod]
    public void Lexer_Keyword_MapsToSpecificKind()
    {
        var tokens = new Lexer("forward back turn move line rect").Tokenize();

        Assert.AreEqual(TokenKind.Forward, tokens[0].Kind);
        Assert.AreEqual(TokenKind.Back, tokens[1].Kind);
        Assert.AreEqual(TokenKind.Turn, tokens[2].Kind);
        Assert.AreEqual(TokenKind.Move, tokens[3].Kind);
        Assert.AreEqual(TokenKind.Line, tokens[4].Kind);
        Assert.AreEqual(TokenKind.Rect, tokens[5].Kind);
    }

    [TestMethod]
    public void Lexer_IntAndFloatLiterals_ParseCorrectly()
    {
        var tokens = new Lexer("42 3.14 0.5").Tokenize();

        Assert.AreEqual(TokenKind.IntLiteral, tokens[0].Kind);
        Assert.AreEqual(42.0, tokens[0].Number);
        Assert.AreEqual(TokenKind.FloatLiteral, tokens[1].Kind);
        Assert.AreEqual(3.14, tokens[1].Number, 1e-9);
        Assert.AreEqual(TokenKind.FloatLiteral, tokens[2].Kind);
        Assert.AreEqual(0.5, tokens[2].Number, 1e-9);
    }

    [TestMethod]
    public void Lexer_Fraction_SplitsNumeratorAndDenominator()
    {
        var tokens = new Lexer("1/40").Tokenize();

        Assert.AreEqual(TokenKind.FractionLiteral, tokens[0].Kind);
        Assert.AreEqual(1, (int)tokens[0].Number);
        Assert.AreEqual(40, tokens[0].SecondValue);
    }

    [TestMethod]
    public void Lexer_DivisionOperator_IsSeparateFromFraction()
    {
        // `x / y` (whitespace) is a binary op; `1/40` (no whitespace + integer) is a fraction.
        var tokens = new Lexer("x / y").Tokenize();

        Assert.AreEqual(TokenKind.Identifier, tokens[0].Kind);
        Assert.AreEqual(TokenKind.Slash, tokens[1].Kind);
        Assert.AreEqual(TokenKind.Identifier, tokens[2].Kind);
    }

    [TestMethod]
    public void Lexer_String_HandlesEscapes()
    {
        var tokens = new Lexer("\"Hello\\nWorld\"").Tokenize();

        Assert.AreEqual(TokenKind.StringLiteral, tokens[0].Kind);
        Assert.AreEqual("Hello\nWorld", tokens[0].Lexeme);
    }

    [TestMethod]
    public void Lexer_Comment_IsDiscarded()
    {
        var tokens = new Lexer("forward 10 // keep going\nback 5").Tokenize();

        Assert.AreEqual(TokenKind.Forward, tokens[0].Kind);
        Assert.AreEqual(TokenKind.IntLiteral, tokens[1].Kind);
        Assert.AreEqual(TokenKind.Back, tokens[2].Kind);
        Assert.AreEqual(TokenKind.IntLiteral, tokens[3].Kind);
    }

    [TestMethod]
    public void Lexer_Directive_IsDistinguishedFromKeyword()
    {
        var tokens = new Lexer("#coord pixels").Tokenize();

        Assert.AreEqual(TokenKind.Directive, tokens[0].Kind);
        Assert.AreEqual("coord", tokens[0].Lexeme);
        Assert.AreEqual(TokenKind.Identifier, tokens[1].Kind);
    }

    [TestMethod]
    public void Lexer_DotDotRange_ParsesAsSingleToken()
    {
        var tokens = new Lexer("1..10").Tokenize();

        Assert.AreEqual(TokenKind.IntLiteral, tokens[0].Kind);
        Assert.AreEqual(TokenKind.DotDot, tokens[1].Kind);
        Assert.AreEqual(TokenKind.IntLiteral, tokens[2].Kind);
    }

    [TestMethod]
    public void Lexer_ShorthandAliases_MapToCanonicalKinds()
    {
        var tokens = new Lexer("fd bk poly").Tokenize();

        Assert.AreEqual(TokenKind.Forward, tokens[0].Kind);
        Assert.AreEqual(TokenKind.Back, tokens[1].Kind);
        Assert.AreEqual(TokenKind.Polygon, tokens[2].Kind);
    }

    [TestMethod]
    public void Lexer_LineAndColumn_TrackCorrectly()
    {
        var tokens = new Lexer("move\n  forward 10").Tokenize();

        Assert.AreEqual(1, tokens[0].Line);
        Assert.AreEqual(1, tokens[0].Column);
        Assert.AreEqual(2, tokens[1].Line);
        Assert.AreEqual(3, tokens[1].Column);
    }

    [TestMethod]
    public void Lexer_UnterminatedString_EmitsDiagnostic()
    {
        var lexer = new Lexer("\"no close");
        lexer.Tokenize();

        Assert.AreEqual(1, lexer.Diagnostics.Count);
        Assert.AreEqual(DiagnosticSeverity.Error, lexer.Diagnostics[0].Severity);
    }
}
