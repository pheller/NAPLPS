// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPS.Telidraw;

/// <summary>
/// Hand-rolled Telidraw lexer. Single-pass; emits a flat <see cref="Token"/> stream
/// terminated by <see cref="TokenKind.Eof"/>. Any lexical errors are collected in
/// <see cref="Diagnostics"/> and the lexer continues so the parser can report a
/// reasonable tail of issues in one run.
/// </summary>
public sealed class Lexer
{
    private static readonly Dictionary<string, TokenKind> Keywords = new(StringComparer.Ordinal)
    {
        // Structural
        ["proc"] = TokenKind.Proc,
        ["with"] = TokenKind.With,
        ["repeat"] = TokenKind.Repeat,
        ["for"] = TokenKind.For,
        ["in"] = TokenKind.In,
        ["if"] = TokenKind.If,
        ["else"] = TokenKind.Else,
        ["palette"] = TokenKind.Palette,
        ["let"] = TokenKind.Let,

        // Drawing verbs
        ["forward"] = TokenKind.Forward,
        ["fd"] = TokenKind.Forward,             // Logo-style shorthand
        ["back"] = TokenKind.Back,
        ["bk"] = TokenKind.Back,
        ["turn"] = TokenKind.Turn,
        ["move"] = TokenKind.Move,
        ["goto"] = TokenKind.Goto,
        ["line"] = TokenKind.Line,
        ["rect"] = TokenKind.Rect,
        ["arc"] = TokenKind.Arc,
        ["polygon"] = TokenKind.Polygon,
        ["poly"] = TokenKind.Polygon,
        ["point"] = TokenKind.Point,
        ["text"] = TokenKind.Text,
        ["color"] = TokenKind.Color,
        ["texture"] = TokenKind.Texture,
        ["domain"] = TokenKind.Domain,
        ["blink"] = TokenKind.Blink,
        ["wait"] = TokenKind.Wait,
        ["reset"] = TokenKind.Reset,
        ["drcs"] = TokenKind.Drcs,
        ["field"] = TokenKind.Field,
        ["scribble"] = TokenKind.Scribble,
        ["bitmap"] = TokenKind.Bitmap,
        ["close"] = TokenKind.Close,
        ["raw"] = TokenKind.Raw,
    };

    private readonly string _source;
    private readonly List<Token> _tokens = [];
    private readonly List<Diagnostic> _diagnostics = [];

    private int _pos;
    private int _line = 1;
    private int _column = 1;

    public Lexer(string source)
    {
        _source = source ?? string.Empty;
    }

    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    /// <summary>Run the lexer once and return all tokens (including the final Eof).</summary>
    public IReadOnlyList<Token> Tokenize()
    {
        while (!IsAtEnd())
        {
            ScanToken();
        }

        _tokens.Add(new Token(TokenKind.Eof, string.Empty, _line, _column));
        return _tokens;
    }

    private void ScanToken()
    {
        int startLine = _line;
        int startCol = _column;
        char c = Advance();

        switch (c)
        {
            case ' ':
            case '\t':
            case '\r':
                return;

            case '\n':
                _line++;
                _column = 1;
                return;

            case '/' when Peek() == '/':
                // Line comment: consume until newline.
                while (!IsAtEnd() && Peek() != '\n')
                {
                    Advance();
                }
                return;

            case '/':
                Emit(TokenKind.Slash, "/", startLine, startCol);
                return;

            case '#':
                ScanDirective(startLine, startCol);
                return;

            case '(': Emit(TokenKind.LParen, "(", startLine, startCol); return;
            case ')': Emit(TokenKind.RParen, ")", startLine, startCol); return;
            case '{': Emit(TokenKind.LBrace, "{", startLine, startCol); return;
            case '}': Emit(TokenKind.RBrace, "}", startLine, startCol); return;
            case '[': Emit(TokenKind.LBracket, "[", startLine, startCol); return;
            case ']': Emit(TokenKind.RBracket, "]", startLine, startCol); return;
            case ',': Emit(TokenKind.Comma, ",", startLine, startCol); return;
            case ':': Emit(TokenKind.Colon, ":", startLine, startCol); return;
            case ';': Emit(TokenKind.Semicolon, ";", startLine, startCol); return;
            case '+': Emit(TokenKind.Plus, "+", startLine, startCol); return;
            case '-':
                if (char.IsDigit(Peek()))
                {
                    ScanNumber('-', startLine, startCol);
                }
                else
                {
                    Emit(TokenKind.Minus, "-", startLine, startCol);
                }
                return;
            case '*': Emit(TokenKind.Star, "*", startLine, startCol); return;
            case '%': Emit(TokenKind.Percent, "%", startLine, startCol); return;
            case '=': Emit(TokenKind.Equals, "=", startLine, startCol); return;
            case '@': Emit(TokenKind.At, "@", startLine, startCol); return;

            case '.':
                if (Peek() == '.')
                {
                    Advance();
                    Emit(TokenKind.DotDot, "..", startLine, startCol);
                }
                else
                {
                    // Allow leading-dot floats: .5 → 0.5
                    if (char.IsDigit(Peek()))
                    {
                        ScanNumber('0', startLine, startCol, includeDot: true);
                    }
                    else
                    {
                        _diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, startLine, startCol, "Unexpected '.'", "Did you mean '..'?"));
                    }
                }
                return;

            case '"':
                ScanString(startLine, startCol);
                return;

            default:
                if (char.IsDigit(c))
                {
                    ScanNumber(c, startLine, startCol);
                }
                else if (IsIdentStart(c))
                {
                    ScanIdentifier(c, startLine, startCol);
                }
                else
                {
                    _diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, startLine, startCol, $"Unexpected character '{c}'"));
                }
                return;
        }
    }

    private void ScanDirective(int startLine, int startCol)
    {
        // Directive: #name[ args...]. The parser reads any args as expressions; lexer just
        // emits the directive-name token and lets subsequent whitespace-separated tokens flow.
        var name = new System.Text.StringBuilder();

        while (!IsAtEnd() && IsIdentPart(Peek()))
        {
            name.Append(Advance());
        }

        _tokens.Add(new Token(TokenKind.Directive, name.ToString(), startLine, startCol));
    }

    private void ScanString(int startLine, int startCol)
    {
        var sb = new System.Text.StringBuilder();

        while (!IsAtEnd() && Peek() != '"')
        {
            char c = Advance();
            if (c == '\\' && !IsAtEnd())
            {
                char esc = Advance();
                sb.Append(esc switch
                {
                    'n' => '\n',
                    't' => '\t',
                    'r' => '\r',
                    '\\' => '\\',
                    '"' => '"',
                    _ => esc,
                });
            }
            else if (c == '\n')
            {
                _line++;
                _column = 1;
                sb.Append('\n');
            }
            else
            {
                sb.Append(c);
            }
        }

        if (IsAtEnd())
        {
            _diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, startLine, startCol, "Unterminated string literal"));
            return;
        }

        Advance(); // closing quote
        _tokens.Add(new Token(TokenKind.StringLiteral, sb.ToString(), startLine, startCol));
    }

    private void ScanNumber(char first, int startLine, int startCol, bool includeDot = false)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(first);

        if (includeDot)
        {
            sb.Append('.');
        }

        bool hasDot = includeDot;

        while (!IsAtEnd())
        {
            char c = Peek();

            if (char.IsDigit(c))
            {
                sb.Append(Advance());
            }
            else if (c == '.' && !hasDot && Peek(1) != '.')
            {
                hasDot = true;
                sb.Append(Advance());
            }
            else
            {
                break;
            }
        }

        var numText = sb.ToString();

        // Fraction form N/M — only when the slash is immediately followed by another digit
        // (avoid swallowing the comment marker or a divide operator).
        if (!hasDot && Peek() == '/' && char.IsDigit(Peek(1)))
        {
            Advance(); // consume '/'
            var denom = new System.Text.StringBuilder();

            while (!IsAtEnd() && char.IsDigit(Peek()))
            {
                denom.Append(Advance());
            }

            if (int.TryParse(numText, out var n) && int.TryParse(denom.ToString(), out var d) && d != 0)
            {
                _tokens.Add(new Token(TokenKind.FractionLiteral, $"{n}/{d}", startLine, startCol, n, d));
                return;
            }

            _diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, startLine, startCol, $"Invalid fraction '{numText}/{denom}'"));
            return;
        }

        if (hasDot)
        {
            if (double.TryParse(numText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var f))
            {
                _tokens.Add(new Token(TokenKind.FloatLiteral, numText, startLine, startCol, f));
            }
            else
            {
                _diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, startLine, startCol, $"Invalid float literal '{numText}'"));
            }
        }
        else
        {
            if (long.TryParse(numText, out var i))
            {
                _tokens.Add(new Token(TokenKind.IntLiteral, numText, startLine, startCol, i));
            }
            else
            {
                _diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, startLine, startCol, $"Invalid integer literal '{numText}'"));
            }
        }
    }

    private void ScanIdentifier(char first, int startLine, int startCol)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(first);

        while (!IsAtEnd() && IsIdentPart(Peek()))
        {
            sb.Append(Advance());
        }

        var text = sb.ToString();
        var kind = Keywords.TryGetValue(text, out var kw) ? kw : TokenKind.Identifier;
        _tokens.Add(new Token(kind, text, startLine, startCol));
    }

    private void Emit(TokenKind kind, string lexeme, int line, int col)
    {
        _tokens.Add(new Token(kind, lexeme, line, col));
    }

    private bool IsAtEnd() => _pos >= _source.Length;

    private char Advance()
    {
        char c = _source[_pos++];
        _column++;
        return c;
    }

    private char Peek(int offset = 0)
    {
        int idx = _pos + offset;
        return idx < _source.Length ? _source[idx] : '\0';
    }

    private static bool IsIdentStart(char c) => char.IsLetter(c) || c == '_';

    private static bool IsIdentPart(char c) => char.IsLetterOrDigit(c) || c == '_';
}
