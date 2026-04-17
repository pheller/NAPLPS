// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Telidraw;

/// <summary>
/// Recursive-descent Telidraw parser. Consumes a token stream from <see cref="Lexer"/>
/// and produces a <see cref="ProgramNode"/>. Expression parsing uses Pratt-style
/// precedence climbing. The parser is error-tolerant: it records diagnostics and
/// attempts to skip to the next statement boundary on failure so one run surfaces
/// multiple issues.
/// </summary>
public sealed class Parser
{
    private static readonly HashSet<TokenKind> CommandVerbs =
    [
        TokenKind.Forward, TokenKind.Back, TokenKind.Turn,
        TokenKind.Move, TokenKind.MoveRel, TokenKind.Goto,
        TokenKind.Line, TokenKind.LineRel, TokenKind.LineSet, TokenKind.LineSetRel,
        TokenKind.Rect, TokenKind.RectOutline, TokenKind.RectSet, TokenKind.RectSetOutline,
        TokenKind.Arc, TokenKind.ArcOutline, TokenKind.ArcSet, TokenKind.ArcSetOutline,
        TokenKind.Polygon, TokenKind.PolyOutline, TokenKind.PolySet, TokenKind.PolySetOutline,
        TokenKind.Point, TokenKind.PointRel,
        TokenKind.Text, TokenKind.Color, TokenKind.SetColor, TokenKind.Texture, TokenKind.Domain,
        TokenKind.Blink, TokenKind.Wait, TokenKind.Reset, TokenKind.Nsr,
        TokenKind.Drcs, TokenKind.Field, TokenKind.Scribble, TokenKind.Bitmap,
        TokenKind.Close, TokenKind.Raw,
    ];

    private static readonly HashSet<TokenKind> WithAttributes =
    [
        TokenKind.Color, TokenKind.Texture, TokenKind.Domain,
    ];

    private readonly IReadOnlyList<Token> _tokens;
    private readonly List<Diagnostic> _diagnostics = [];

    private int _pos;

    public Parser(IReadOnlyList<Token> tokens)
    {
        _tokens = tokens;
    }

    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    public ProgramNode Parse()
    {
        var directives = new List<DirectiveNode>();
        var statements = new List<StatementNode>();

        // Hoist all leading directives to the program header — after the first non-directive
        // statement, directives get parsed as regular statements with an error.
        while (Peek().Kind == TokenKind.Directive)
        {
            directives.Add(ParseDirective());
        }

        while (Peek().Kind != TokenKind.Eof)
        {
            var stmt = TryParseStatement();

            if (stmt != null)
            {
                statements.Add(stmt);
            }
        }

        return new ProgramNode(directives, statements);
    }

    // ---- Statements ----------------------------------------------------

    private StatementNode? TryParseStatement()
    {
        try
        {
            return ParseStatement();
        }
        catch (ParseError)
        {
            // Resync: skip until the next statement-ish boundary so the next error is reported.
            while (Peek().Kind != TokenKind.Eof && Peek().Kind != TokenKind.RBrace)
            {
                Advance();

                if (Previous().Kind == TokenKind.Semicolon)
                {
                    break;
                }
            }
            return null;
        }
    }

    private StatementNode ParseStatement()
    {
        var tok = Peek();

        return tok.Kind switch
        {
            TokenKind.Proc => ParseProcDecl(asMacro: false),
            TokenKind.At => ParseMacroProc(),
            TokenKind.With => ParseWithBlock(),
            TokenKind.Repeat => ParseRepeat(),
            TokenKind.For => ParseForLoop(),
            TokenKind.If => ParseIf(),
            TokenKind.Palette => ParsePaletteAlias(),
            TokenKind.Let => ParseLet(),
            TokenKind.Directive => ParseDirective(),
            TokenKind.Raw => ParseRawStatement(),
            _ when CommandVerbs.Contains(tok.Kind) => ParseCommandCall(),
            TokenKind.Identifier => ParseProcCall(),
            _ => throw Error(tok, $"Expected a statement, got '{tok}'"),
        };
    }

    private StatementNode ParseMacroProc()
    {
        var at = Expect(TokenKind.At);

        if (Peek().Kind == TokenKind.Identifier && Peek().Lexeme == "macro")
        {
            Advance(); // 'macro'
            Expect(TokenKind.Proc);
            return ParseProcDeclAfterKeyword(asMacro: true, line: at.Line, column: at.Column);
        }

        throw Error(at, "Expected '@macro' before 'proc'");
    }

    private ProcDeclNode ParseProcDecl(bool asMacro)
    {
        var proc = Expect(TokenKind.Proc);
        return ParseProcDeclAfterKeyword(asMacro, proc.Line, proc.Column);
    }

    private ProcDeclNode ParseProcDeclAfterKeyword(bool asMacro, int line, int column)
    {
        var name = Expect(TokenKind.Identifier).Lexeme;
        Expect(TokenKind.LParen);

        var parameters = new List<string>();
        if (Peek().Kind != TokenKind.RParen)
        {
            parameters.Add(Expect(TokenKind.Identifier).Lexeme);
            while (Match(TokenKind.Comma))
            {
                parameters.Add(Expect(TokenKind.Identifier).Lexeme);
            }
        }

        Expect(TokenKind.RParen);
        var body = ParseBlock();
        return new ProcDeclNode(name, parameters, body, asMacro, line, column);
    }

    private WithBlockNode ParseWithBlock()
    {
        var withTok = Expect(TokenKind.With);
        var attrTok = Advance();

        if (!WithAttributes.Contains(attrTok.Kind))
        {
            throw Error(attrTok, $"'with' expects one of {string.Join(", ", WithAttributes)}");
        }

        var args = new List<ExpressionNode>();
        while (!IsBlockStart() && Peek().Kind != TokenKind.Eof)
        {
            args.Add(ParseExpression());
        }

        var body = ParseBlock();
        return new WithBlockNode(attrTok.Kind, args, body, withTok.Line, withTok.Column);
    }

    private RepeatNode ParseRepeat()
    {
        var tok = Expect(TokenKind.Repeat);
        var count = ParseExpression();
        var body = ParseBlock();
        return new RepeatNode(count, body, tok.Line, tok.Column);
    }

    private ForNode ParseForLoop()
    {
        var tok = Expect(TokenKind.For);
        var variable = Expect(TokenKind.Identifier).Lexeme;
        Expect(TokenKind.In);
        var from = ParseExpression();
        Expect(TokenKind.DotDot);
        var to = ParseExpression();
        var body = ParseBlock();
        return new ForNode(variable, from, to, body, tok.Line, tok.Column);
    }

    private IfNode ParseIf()
    {
        var tok = Expect(TokenKind.If);
        var cond = ParseExpression();
        var thenBody = ParseBlock();
        List<StatementNode>? elseBody = null;

        if (Match(TokenKind.Else))
        {
            elseBody = ParseBlock();
        }

        return new IfNode(cond, thenBody, elseBody, tok.Line, tok.Column);
    }

    private PaletteAliasNode ParsePaletteAlias()
    {
        var tok = Expect(TokenKind.Palette);
        var name = Expect(TokenKind.Identifier).Lexeme;
        Expect(TokenKind.Equals);
        var value = ParseExpression();
        return new PaletteAliasNode(name, value, tok.Line, tok.Column);
    }

    private LetNode ParseLet()
    {
        var tok = Expect(TokenKind.Let);
        var name = Expect(TokenKind.Identifier).Lexeme;
        Expect(TokenKind.Equals);
        var value = ParseExpression();
        return new LetNode(name, value, tok.Line, tok.Column);
    }

    private DirectiveNode ParseDirective()
    {
        var tok = Expect(TokenKind.Directive);
        var args = new List<ExpressionNode>();

        // Directives accept any whitespace-separated expressions up to the next statement-ish boundary.
        while (IsExpressionStart() && Peek().Kind != TokenKind.Directive)
        {
            args.Add(ParseExpression());
        }

        return new DirectiveNode(tok.Lexeme, args, tok.Line, tok.Column);
    }

    /// <summary>
    /// Parse `raw 164 192 210 192` — reads integer expressions until the next
    /// non-expression token, packs them as bytes into a <see cref="RawStatementNode"/>.
    /// First byte is the opcode; rest are operand bytes.
    /// </summary>
    private RawStatementNode ParseRawStatement()
    {
        var tok = Expect(TokenKind.Raw);
        var args = new List<ExpressionNode>();

        while (IsExpressionStart())
        {
            args.Add(ParseExpression());
        }

        // The compiler resolves expressions at compile time; here we just store
        // the expressions. The compiler's CompileRaw evaluates and packs.
        return new RawStatementNode(args.Select(a =>
        {
            if (a is NumberLiteralNode n) return (byte)((int)n.Value & 0xFF);
            // For non-literal expressions, evaluate later; store 0 as placeholder.
            return (byte)0;
        }).ToList(), tok.Line, tok.Column);
    }

    private CommandCallNode ParseCommandCall()
    {
        var tok = Advance();
        var args = new List<ExpressionNode>();

        while (IsExpressionStart())
        {
            args.Add(ParseExpression());
        }

        return new CommandCallNode(tok.Kind, args, tok.Line, tok.Column);
    }

    private ProcCallNode ParseProcCall()
    {
        var name = Expect(TokenKind.Identifier);
        var args = new List<ExpressionNode>();

        while (IsExpressionStart())
        {
            args.Add(ParseExpression());
        }

        return new ProcCallNode(name.Lexeme, args, name.Line, name.Column);
    }

    private List<StatementNode> ParseBlock()
    {
        Expect(TokenKind.LBrace);
        var body = new List<StatementNode>();

        while (Peek().Kind != TokenKind.RBrace && Peek().Kind != TokenKind.Eof)
        {
            var stmt = TryParseStatement();
            if (stmt != null)
            {
                body.Add(stmt);
            }
        }

        Expect(TokenKind.RBrace);
        return body;
    }

    // ---- Expressions (Pratt precedence climbing) ----------------------

    private static int GetBinaryPrecedence(TokenKind kind)
    {
        return kind switch
        {
            TokenKind.Plus or TokenKind.Minus => 10,
            TokenKind.Star or TokenKind.Slash or TokenKind.Percent => 20,
            _ => 0,
        };
    }

    private ExpressionNode ParseExpression(int minPrecedence = 0)
    {
        var left = ParseUnary();

        while (true)
        {
            int prec = GetBinaryPrecedence(Peek().Kind);
            if (prec == 0 || prec < minPrecedence)
            {
                break;
            }

            var op = Advance();
            var right = ParseExpression(prec + 1);
            left = new BinaryOpNode(op.Lexeme, left, right, op.Line, op.Column);
        }

        return left;
    }

    private ExpressionNode ParseUnary()
    {
        if (Peek().Kind == TokenKind.Minus || Peek().Kind == TokenKind.Plus)
        {
            var op = Advance();
            var operand = ParseUnary();
            return new UnaryOpNode(op.Lexeme, operand, op.Line, op.Column);
        }

        return ParsePrimary();
    }

    private ExpressionNode ParsePrimary()
    {
        var tok = Peek();

        switch (tok.Kind)
        {
            case TokenKind.IntLiteral:
                Advance();
                return new NumberLiteralNode(tok.Number, tok.Line, tok.Column);

            case TokenKind.FloatLiteral:
                Advance();
                return new NumberLiteralNode(tok.Number, tok.Line, tok.Column);

            case TokenKind.FractionLiteral:
                Advance();
                return new FractionLiteralNode((int)tok.Number, tok.SecondValue, tok.Line, tok.Column);

            case TokenKind.StringLiteral:
                Advance();
                return new StringLiteralNode(tok.Lexeme, tok.Line, tok.Column);

            case TokenKind.Identifier:
                Advance();
                // Call form: name(arg1, arg2) — for now not used heavily; main commands use no-paren calls.
                if (Peek().Kind == TokenKind.LParen)
                {
                    Advance();
                    var callArgs = new List<ExpressionNode>();

                    if (Peek().Kind != TokenKind.RParen)
                    {
                        callArgs.Add(ParseExpression());
                        while (Match(TokenKind.Comma))
                        {
                            callArgs.Add(ParseExpression());
                        }
                    }

                    Expect(TokenKind.RParen);
                    return new CallExpressionNode(tok.Lexeme, callArgs, tok.Line, tok.Column);
                }

                return new IdentifierNode(tok.Lexeme, tok.Line, tok.Column);

            case TokenKind.LParen:
                Advance();
                var inner = ParseExpression();
                Expect(TokenKind.RParen);
                return inner;

            default:
                throw Error(tok, $"Expected an expression, got '{tok}'");
        }
    }

    // ---- Helpers ------------------------------------------------------

    private bool IsExpressionStart()
    {
        return Peek().Kind switch
        {
            TokenKind.IntLiteral or TokenKind.FloatLiteral or TokenKind.FractionLiteral
                or TokenKind.StringLiteral or TokenKind.Identifier
                or TokenKind.LParen or TokenKind.Minus or TokenKind.Plus => true,
            _ => false,
        };
    }

    private bool IsBlockStart() => Peek().Kind == TokenKind.LBrace;

    private Token Peek(int offset = 0) => _pos + offset < _tokens.Count ? _tokens[_pos + offset] : _tokens[^1];

    private Token Previous() => _tokens[_pos - 1];

    private Token Advance() => _tokens[_pos++];

    private bool Match(TokenKind kind)
    {
        if (Peek().Kind == kind)
        {
            Advance();
            return true;
        }
        return false;
    }

    private Token Expect(TokenKind kind)
    {
        var tok = Peek();
        if (tok.Kind != kind)
        {
            throw Error(tok, $"Expected {kind}, got {tok.Kind}");
        }
        Advance();
        return tok;
    }

    private ParseError Error(Token tok, string message)
    {
        _diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, tok.Line, tok.Column, message));
        return new ParseError();
    }

    private sealed class ParseError : Exception { }
}
