// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Globalization;
using System.Text;

namespace NAPLPS.Telidraw;

/// <summary>
/// Emits valid Telidraw source text from an AST. Used for:
///   1. Debug dumping (so tests can compare printed form to an expected string)
///   2. The Phase 8 decompiler, which builds an AST from parsed NAPLPS commands and
///      then prints it here — the same code path the human-authored source goes through.
/// Output uses 2-space indentation and a single space between command tokens.
/// </summary>
public sealed class AstPrinter
{
    private readonly StringBuilder _sb = new();
    private int _indent;

    private const string IndentUnit = "  ";

    public static string Print(ProgramNode program)
    {
        var printer = new AstPrinter();
        printer.VisitProgram(program);
        return printer._sb.ToString();
    }

    private void VisitProgram(ProgramNode program)
    {
        foreach (var d in program.Directives)
        {
            VisitDirective(d);
        }

        if (program.Directives.Count > 0 && program.Statements.Count > 0)
        {
            _sb.AppendLine();
        }

        foreach (var s in program.Statements)
        {
            VisitStatement(s);
        }
    }

    private void VisitStatement(StatementNode stmt)
    {
        switch (stmt)
        {
            case DirectiveNode d: VisitDirective(d); break;
            case CommandCallNode c: VisitCommandCall(c); break;
            case ProcCallNode p: VisitProcCall(p); break;
            case WithBlockNode w: VisitWithBlock(w); break;
            case RepeatNode r: VisitRepeat(r); break;
            case ForNode f: VisitFor(f); break;
            case IfNode i: VisitIf(i); break;
            case ProcDeclNode pd: VisitProcDecl(pd); break;
            case PaletteAliasNode pa: VisitPaletteAlias(pa); break;
            case LetNode l: VisitLet(l); break;
            case RawStatementNode raw: VisitRaw(raw); break;
            default: Indent(); _sb.AppendLine($"/* unknown statement: {stmt.GetType().Name} */"); break;
        }
    }

    private void VisitDirective(DirectiveNode d)
    {
        Indent();
        _sb.Append('#').Append(d.Name);

        foreach (var arg in d.Args)
        {
            _sb.Append(' ');
            VisitExpression(arg);
        }

        _sb.AppendLine();
    }

    private void VisitCommandCall(CommandCallNode c)
    {
        Indent();
        _sb.Append(KeywordText(c.Command));

        foreach (var arg in c.Args)
        {
            _sb.Append(' ');
            VisitExpression(arg);
        }

        _sb.AppendLine();
    }

    private void VisitProcCall(ProcCallNode p)
    {
        Indent();
        _sb.Append(p.Name);

        foreach (var arg in p.Args)
        {
            _sb.Append(' ');
            VisitExpression(arg);
        }

        _sb.AppendLine();
    }

    private void VisitWithBlock(WithBlockNode w)
    {
        Indent();
        _sb.Append("with ").Append(KeywordText(w.Attribute));

        foreach (var arg in w.AttributeArgs)
        {
            _sb.Append(' ');
            VisitExpression(arg);
        }

        _sb.AppendLine(" {");
        _indent++;

        foreach (var s in w.Body)
        {
            VisitStatement(s);
        }

        _indent--;
        Indent();
        _sb.AppendLine("}");
    }

    private void VisitRepeat(RepeatNode r)
    {
        Indent();
        _sb.Append("repeat ");
        VisitExpression(r.Count);
        _sb.AppendLine(" {");
        _indent++;

        foreach (var s in r.Body)
        {
            VisitStatement(s);
        }

        _indent--;
        Indent();
        _sb.AppendLine("}");
    }

    private void VisitFor(ForNode f)
    {
        Indent();
        _sb.Append("for ").Append(f.Variable).Append(" in ");
        VisitExpression(f.From);
        _sb.Append("..");
        VisitExpression(f.To);
        _sb.AppendLine(" {");
        _indent++;

        foreach (var s in f.Body)
        {
            VisitStatement(s);
        }

        _indent--;
        Indent();
        _sb.AppendLine("}");
    }

    private void VisitIf(IfNode i)
    {
        Indent();
        _sb.Append("if ");
        VisitExpression(i.Condition);
        _sb.AppendLine(" {");
        _indent++;

        foreach (var s in i.Then)
        {
            VisitStatement(s);
        }

        _indent--;
        Indent();

        if (i.Else != null)
        {
            _sb.AppendLine("} else {");
            _indent++;

            foreach (var s in i.Else)
            {
                VisitStatement(s);
            }

            _indent--;
            Indent();
        }

        _sb.AppendLine("}");
    }

    private void VisitProcDecl(ProcDeclNode pd)
    {
        Indent();

        if (pd.AsMacro)
        {
            _sb.Append("@macro ");
        }

        _sb.Append("proc ").Append(pd.Name).Append('(');
        _sb.Append(string.Join(", ", pd.Parameters));
        _sb.AppendLine(") {");
        _indent++;

        foreach (var s in pd.Body)
        {
            VisitStatement(s);
        }

        _indent--;
        Indent();
        _sb.AppendLine("}");
    }

    private void VisitPaletteAlias(PaletteAliasNode pa)
    {
        Indent();
        _sb.Append("palette ").Append(pa.Name).Append(" = ");
        VisitExpression(pa.Value);
        _sb.AppendLine();
    }

    private void VisitLet(LetNode l)
    {
        Indent();
        _sb.Append("let ").Append(l.Name).Append(" = ");
        VisitExpression(l.Value);
        _sb.AppendLine();
    }

    private void VisitRaw(RawStatementNode raw)
    {
        Indent();
        _sb.Append("raw");

        foreach (var b in raw.Bytes)
        {
            _sb.Append(' ').Append(b);
        }

        _sb.AppendLine();
    }

    private void VisitExpression(ExpressionNode expr)
    {
        switch (expr)
        {
            case NumberLiteralNode n:
                _sb.Append(FormatNumber(n.Value));
                break;

            case FractionLiteralNode f:
                _sb.Append(f.Numerator).Append('/').Append(f.Denominator);
                break;

            case StringLiteralNode s:
                _sb.Append('"').Append(s.Value.Replace("\\", "\\\\").Replace("\"", "\\\"")).Append('"');
                break;

            case IdentifierNode i:
                _sb.Append(i.Name);
                break;

            case BinaryOpNode b:
                _sb.Append('(');
                VisitExpression(b.Left);
                _sb.Append(' ').Append(b.Op).Append(' ');
                VisitExpression(b.Right);
                _sb.Append(')');
                break;

            case UnaryOpNode u:
                _sb.Append(u.Op);
                VisitExpression(u.Operand);
                break;

            case CallExpressionNode c:
                _sb.Append(c.Name).Append('(');
                for (int k = 0; k < c.Args.Count; k++)
                {
                    if (k > 0) _sb.Append(", ");
                    VisitExpression(c.Args[k]);
                }
                _sb.Append(')');
                break;
        }
    }

    private static string FormatNumber(double value)
    {
        // Whole numbers print without decimal point; others print invariant-culture decimal.
        if (value == Math.Truncate(value) && !double.IsInfinity(value))
        {
            return ((long)value).ToString(CultureInfo.InvariantCulture);
        }

        return value.ToString("G", CultureInfo.InvariantCulture);
    }

    private static string KeywordText(TokenKind kind)
    {
        return kind switch
        {
            TokenKind.Forward => "forward",
            TokenKind.Back => "back",
            TokenKind.Turn => "turn",
            TokenKind.Move => "move",
            TokenKind.Goto => "goto",
            TokenKind.Line => "line",
            TokenKind.Rect => "rect",
            TokenKind.Arc => "arc",
            TokenKind.Polygon => "polygon",
            TokenKind.Point => "point",
            TokenKind.Text => "text",
            TokenKind.Color => "color",
            TokenKind.Texture => "texture",
            TokenKind.Domain => "domain",
            TokenKind.Blink => "blink",
            TokenKind.Wait => "wait",
            TokenKind.Reset => "reset",
            TokenKind.Drcs => "drcs",
            TokenKind.Field => "field",
            TokenKind.Scribble => "scribble",
            TokenKind.Bitmap => "bitmap",
            TokenKind.Close => "close",
            _ => kind.ToString().ToLowerInvariant(),
        };
    }

    private void Indent()
    {
        for (int k = 0; k < _indent; k++)
        {
            _sb.Append(IndentUnit);
        }
    }
}
