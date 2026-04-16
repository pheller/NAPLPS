// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Numerics;

namespace NAPLPS.Telidraw;

/// <summary>
/// Coordinate system for numeric literals in Telidraw source.
/// <list type="bullet">
///   <item><see cref="Fractions"/> \u2014 literals are already in unit-screen coords (0..1, 0..0.75).</item>
///   <item><see cref="Pixels"/> \u2014 literals are in the current domain's pixel space and get
///     divided by the domain resolution before being fed to <see cref="NaplpsCommandBuilder"/>.</item>
/// </list>
/// </summary>
public enum CoordMode
{
    Fractions,
    Pixels,
}

/// <summary>
/// Turtle-graphics state used by <c>forward</c> / <c>back</c> / <c>turn</c>.
/// Heading is in degrees, 0 = +X (right), 90 = +Y (up), matching the NAPLPS
/// convention where Y increases upward.
/// </summary>
public record struct TurtleState(Vector3 Position, float HeadingDegrees);

/// <summary>
/// Walks a Telidraw <see cref="ProgramNode"/> and emits NAPLPS commands into a
/// <see cref="NaplpsFormat"/> via <see cref="NaplpsCommandBuilder"/>. Per D2, <c>with</c>
/// blocks compile to explicit-restore: the compiler snapshots the relevant attribute,
/// emits the setter, walks the body, then emits a second setter to restore the previous
/// value. Byte round-trip is preserved; the decompiler (Phase 8) emits flat commands and
/// does not reconstruct <c>with</c> blocks.
/// </summary>
public sealed class Compiler
{
    private readonly ProgramNode _program;
    private NaplpsFormat _format;
    private readonly List<Diagnostic> _diagnostics = [];

    private readonly Dictionary<string, double> _scopeVars = new(StringComparer.Ordinal);
    private readonly Dictionary<string, double> _paletteAliases = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ProcDeclNode> _procs = new(StringComparer.Ordinal);

    private TurtleState _turtle = new(Vector3.Zero, 0f);

    // Currently-active foreground color index (0-15). Tracked so `with color X { }` can
    // emit a restore SelectColor on block exit even if the body changed colors.
    private byte _currentColor = 7; // nominal white

    private CoordMode _coordMode = CoordMode.Fractions;
    private int _domainPixelWidth = 256;
    private int _domainPixelHeight = 192;

    /// <summary>
    /// When true, the compiler creates a bare NaplpsFormat with no CAN+NSR sentinels.
    /// Used by the decompile→recompile round-trip path where the .td source is the
    /// complete byte specification. When false (default for human-authored .td files),
    /// FormatNew sentinels are included.
    /// </summary>
    public bool BareFormat { get; set; }

    public Compiler(ProgramNode program, NaplpsSystemType systemType = NaplpsSystemType.NAPLPS)
    {
        _program = program;
        _systemType = systemType;
        _format = null!; // set in Compile()
    }

    private readonly NaplpsSystemType _systemType;

    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    public NaplpsFormat Compile()
    {
        if (BareFormat)
        {
            _format = new NaplpsFormat(new NaplpsState());
        }
        else
        {
            _format = NaplpsFormat.New(_systemType);
        }

        foreach (var d in _program.Directives)
        {
            ApplyDirective(d);
        }

        // Pre-pass: register all proc declarations so procs can call other procs declared later.
        foreach (var s in _program.Statements)
        {
            if (s is ProcDeclNode proc)
            {
                _procs[proc.Name] = proc;
            }
        }

        foreach (var s in _program.Statements)
        {
            CompileStatement(s);
        }

        return _format;
    }

    // ---- Directives ----------------------------------------------------

    private void ApplyDirective(DirectiveNode d)
    {
        switch (d.Name)
        {
            case "coord":
                if (d.Args.Count >= 1 && d.Args[0] is IdentifierNode id)
                {
                    _coordMode = id.Name switch
                    {
                        "fractions" => CoordMode.Fractions,
                        "pixels" => CoordMode.Pixels,
                        _ => _coordMode,
                    };
                }
                break;

            case "resolution":
                if (d.Args.Count >= 2)
                {
                    _domainPixelWidth = (int)Evaluate(d.Args[0]);
                    _domainPixelHeight = (int)Evaluate(d.Args[1]);
                }
                break;

            default:
                Diag(DiagnosticSeverity.Warning, d.Line, d.Column, $"Unknown directive '#{d.Name}' (ignored)");
                break;
        }
    }

    // ---- Statement dispatch --------------------------------------------

    private void CompileStatement(StatementNode stmt)
    {
        switch (stmt)
        {
            case CommandCallNode c: CompileCommandCall(c); break;
            case ProcCallNode p: CompileProcCall(p); break;
            case WithBlockNode w: CompileWithBlock(w); break;
            case RepeatNode r: CompileRepeat(r); break;
            case ForNode f: CompileFor(f); break;
            case IfNode i: CompileIf(i); break;
            case ProcDeclNode: break; // already registered in pre-pass
            case RawStatementNode raw: CompileRaw(raw); break;
            case PaletteAliasNode pa: _paletteAliases[pa.Name] = Evaluate(pa.Value); break;
            case LetNode l: _scopeVars[l.Name] = Evaluate(l.Value); break;
            case DirectiveNode d: ApplyDirective(d); break;
            default: Diag(DiagnosticSeverity.Error, stmt.Line, stmt.Column, $"Unhandled statement '{stmt.GetType().Name}'"); break;
        }
    }

    // ---- Command dispatch ----------------------------------------------

    private void CompileCommandCall(CommandCallNode c)
    {
        switch (c.Command)
        {
            case TokenKind.Move:
            case TokenKind.Goto:
                ExpectArgs(c, 2);
                EmitMove((float)NormX(Evaluate(c.Args[0])), (float)NormY(Evaluate(c.Args[1])));
                break;

            case TokenKind.Point:
                ExpectArgs(c, 2);
                EmitPoint((float)NormX(Evaluate(c.Args[0])), (float)NormY(Evaluate(c.Args[1])));
                break;

            case TokenKind.Line:
                ExpectArgs(c, 2);
                EmitLine((float)NormX(Evaluate(c.Args[0])), (float)NormY(Evaluate(c.Args[1])));
                break;

            case TokenKind.Forward:
                ExpectArgs(c, 1);
                EmitForward((float)Evaluate(c.Args[0]), draw: true);
                break;

            case TokenKind.Back:
                ExpectArgs(c, 1);
                EmitForward(-(float)Evaluate(c.Args[0]), draw: true);
                break;

            case TokenKind.Turn:
                ExpectArgs(c, 1);
                _turtle.HeadingDegrees += (float)Evaluate(c.Args[0]);
                break;

            case TokenKind.Rect:
                ExpectArgs(c, 2);
                EmitRectFilled((float)NormW(Evaluate(c.Args[0])), (float)NormH(Evaluate(c.Args[1])));
                break;

            case TokenKind.Arc:
                ExpectArgs(c, 4);
                EmitArcFilled(
                    (float)NormX(Evaluate(c.Args[0])), (float)NormY(Evaluate(c.Args[1])),
                    (float)NormX(Evaluate(c.Args[2])), (float)NormY(Evaluate(c.Args[3])));
                break;

            case TokenKind.Polygon:
                if (c.Args.Count < 2 || c.Args.Count % 2 != 0)
                {
                    Diag(DiagnosticSeverity.Error, c.Line, c.Column, $"polygon needs pairs of x,y coords (got {c.Args.Count})");
                    break;
                }
                EmitPolygonFilled(c);
                break;

            case TokenKind.Color:
                if (c.Args.Count == 1)
                {
                    EmitCommand(NaplpsCommandBuilder.BuildSelectColor((byte)Evaluate(c.Args[0])));
                    _currentColor = (byte)Evaluate(c.Args[0]);
                }
                else if (c.Args.Count == 2)
                {
                    EmitCommand(NaplpsCommandBuilder.BuildSelectColor((byte)Evaluate(c.Args[0]), (byte)Evaluate(c.Args[1])));
                    _currentColor = (byte)Evaluate(c.Args[0]);
                }
                else
                {
                    Diag(DiagnosticSeverity.Error, c.Line, c.Column, "color takes 1 (fg) or 2 (fg, bg) args");
                }
                break;

            case TokenKind.Domain:
                // domain singleByteWidth multiByteWidth [dimensionality]
                if (c.Args.Count < 2) { Diag(DiagnosticSeverity.Error, c.Line, c.Column, "'domain' needs at least 2 args"); break; }
                var sv = (byte)Evaluate(c.Args[0]);
                var mv = (byte)Evaluate(c.Args[1]);
                var dim = c.Args.Count >= 3 ? (byte)Evaluate(c.Args[2]) : (byte)2;
                EmitCommand(NaplpsCommandBuilder.BuildDomain(sv, mv, dim));
                break;

            case TokenKind.Texture:
                // texture linePattern highlight fillPattern
                ExpectArgs(c, 3);
                EmitCommand(NaplpsCommandBuilder.BuildTexture(
                    (byte)Evaluate(c.Args[0]),
                    Evaluate(c.Args[1]) != 0,
                    (byte)Evaluate(c.Args[2])));
                break;

            case TokenKind.Wait:
                ExpectArgs(c, 1);
                EmitCommand(NaplpsCommandBuilder.BuildWait((byte)Evaluate(c.Args[0])));
                break;

            case TokenKind.Reset:
                EmitCommand(NaplpsCommandBuilder.BuildReset());
                break;

            case TokenKind.Blink:
                // blink blinkToIndex onInterval offInterval [startDelay]
                if (c.Args.Count < 3)
                {
                    Diag(DiagnosticSeverity.Error, c.Line, c.Column, "blink needs at least (toIndex, onInterval, offInterval)");
                    break;
                }
                var delay = c.Args.Count >= 4 ? (byte)Evaluate(c.Args[3]) : (byte)0;
                EmitCommand(NaplpsCommandBuilder.BuildBlink(
                    (byte)Evaluate(c.Args[0]),
                    ((byte)Evaluate(c.Args[1]), (byte)Evaluate(c.Args[2]), delay)));
                break;

            case TokenKind.Field:
                // field originX originY dimsX dimsY (all optional — empty args = full screen)
                if (c.Args.Count == 0)
                {
                    EmitCommand(NaplpsCommandBuilder.BuildField());
                }
                else if (c.Args.Count == 4)
                {
                    EmitCommand(NaplpsCommandBuilder.BuildField(
                        origin: new Vector3((float)NormX(Evaluate(c.Args[0])), (float)NormY(Evaluate(c.Args[1])), 0),
                        dimensions: new Vector3((float)NormW(Evaluate(c.Args[2])), (float)NormH(Evaluate(c.Args[3])), 0)));
                }
                else
                {
                    Diag(DiagnosticSeverity.Error, c.Line, c.Column, "field takes 0 (full-screen) or 4 (originX originY dimsX dimsY) args");
                }
                break;

            case TokenKind.Text:
                EmitText(c);
                break;

            case TokenKind.Close:
                // Close current polygon path — emit a line back to the last move position.
                // For Phase 7 MVP we approximate as "line to (0,0) if at start"; the compiler
                // doesn't track polygon-start so this is a no-op hint until decompiler adds
                // path-close semantics.
                break;

            default:
                Diag(DiagnosticSeverity.Error, c.Line, c.Column, $"Command '{c.Command}' not yet supported by the compiler");
                break;
        }
    }

    // ---- Emission helpers ----------------------------------------------

    private void EmitMove(float x, float y)
    {
        EmitCommand(NaplpsCommandBuilder.BuildPointSetAbsolute(x, y));
        _turtle.Position = new Vector3(x, y, 0);
    }

    private void EmitPoint(float x, float y)
    {
        EmitCommand(NaplpsCommandBuilder.BuildPointAbsolute(x, y));
        _turtle.Position = new Vector3(x, y, 0);
    }

    private void EmitLine(float x, float y)
    {
        EmitCommand(NaplpsCommandBuilder.BuildLineAbsolute(x, y));
        _turtle.Position = new Vector3(x, y, 0);
    }

    private void EmitForward(float distance, bool draw)
    {
        float rad = _turtle.HeadingDegrees * MathF.PI / 180f;
        float nx = _turtle.Position.X + distance * MathF.Cos(rad);
        float ny = _turtle.Position.Y + distance * MathF.Sin(rad);

        if (draw)
        {
            EmitCommand(NaplpsCommandBuilder.BuildLineAbsolute(nx, ny));
        }
        else
        {
            EmitCommand(NaplpsCommandBuilder.BuildPointSetAbsolute(nx, ny));
        }

        _turtle.Position = new Vector3(nx, ny, 0);
    }

    private void EmitRectFilled(float w, float h)
    {
        EmitCommand(NaplpsCommandBuilder.BuildRectangleFilled(w, h));
        _turtle.Position = new Vector3(_turtle.Position.X + w, _turtle.Position.Y, 0);
    }

    private void EmitArcFilled(float midX, float midY, float endX, float endY)
    {
        // Command operands are (mid relative, end relative) in NAPLPS convention. The
        // Telidraw surface gives absolutes; convert by subtracting pen position.
        float penX = _turtle.Position.X;
        float penY = _turtle.Position.Y;
        EmitCommand(NaplpsCommandBuilder.BuildArcFilled(midX - penX, midY - penY, endX - midX, endY - midY));
        _turtle.Position = new Vector3(endX, endY, 0);
    }

    private void EmitPolygonFilled(CommandCallNode c)
    {
        var verts = new Vector3[c.Args.Count / 2];

        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = new Vector3(
                (float)NormX(Evaluate(c.Args[2 * i])),
                (float)NormY(Evaluate(c.Args[2 * i + 1])),
                0);
        }

        // Convert absolute vertices into relative displacements from the pen — that's the
        // operand shape for PolygonFilled.
        var relVerts = new Vector3[verts.Length];
        var pen = _turtle.Position;

        for (int i = 0; i < verts.Length; i++)
        {
            relVerts[i] = verts[i] - pen;
            pen = verts[i];
        }

        EmitCommand(NaplpsCommandBuilder.BuildPolygonFilled(relVerts));
        _turtle.Position = verts[^1];
    }

    private void EmitText(CommandCallNode c)
    {
        if (c.Args.Count == 0 || c.Args[0] is not StringLiteralNode str)
        {
            Diag(DiagnosticSeverity.Error, c.Line, c.Column, "text expects a string literal as first arg");
            return;
        }

        // Optional [size] argument emits a TEXT command sized accordingly.
        if (c.Args.Count >= 3)
        {
            EmitCommand(NaplpsCommandBuilder.BuildText(
                (float)Evaluate(c.Args[1]),
                (float)Evaluate(c.Args[2])));
        }

        foreach (var ch in str.Value)
        {
            if (ch >= 0x20 && ch <= 0x7E)
            {
                _format.AddCommand((byte)ch, []);
            }
        }
    }

    // ---- With / Repeat / For / If --------------------------------------

    private void CompileWithBlock(WithBlockNode w)
    {
        // D2: emit the new attribute value, walk body, emit the previous value to restore.
        switch (w.Attribute)
        {
            case TokenKind.Color:
            {
                if (w.AttributeArgs.Count == 0)
                {
                    Diag(DiagnosticSeverity.Error, w.Line, w.Column, "with color needs a color index or alias");
                    return;
                }

                var prev = _currentColor;
                var newColor = (byte)Evaluate(w.AttributeArgs[0]);
                EmitCommand(NaplpsCommandBuilder.BuildSelectColor(newColor));
                _currentColor = newColor;

                foreach (var s in w.Body) { CompileStatement(s); }

                EmitCommand(NaplpsCommandBuilder.BuildSelectColor(prev));
                _currentColor = prev;
                break;
            }

            case TokenKind.Texture:
            {
                if (w.AttributeArgs.Count < 3)
                {
                    Diag(DiagnosticSeverity.Error, w.Line, w.Column, "with texture needs (linePattern highlight fillPattern)");
                    return;
                }

                EmitCommand(NaplpsCommandBuilder.BuildTexture(
                    (byte)Evaluate(w.AttributeArgs[0]),
                    Evaluate(w.AttributeArgs[1]) != 0,
                    (byte)Evaluate(w.AttributeArgs[2])));

                foreach (var s in w.Body) { CompileStatement(s); }

                // Restore: default texture (line=solid, no highlight, pattern=0)
                EmitCommand(NaplpsCommandBuilder.BuildTexture(0, false, 0));
                break;
            }

            case TokenKind.Domain:
            {
                if (w.AttributeArgs.Count < 2)
                {
                    Diag(DiagnosticSeverity.Error, w.Line, w.Column, "with domain needs (singleByte multiByte)");
                    return;
                }

                var prevSv = 1; // default single-byte value
                var prevMv = 3; // default multi-byte value

                EmitCommand(NaplpsCommandBuilder.BuildDomain(
                    (byte)Evaluate(w.AttributeArgs[0]),
                    (byte)Evaluate(w.AttributeArgs[1]),
                    w.AttributeArgs.Count >= 3 ? (byte)Evaluate(w.AttributeArgs[2]) : (byte)2));

                foreach (var s in w.Body) { CompileStatement(s); }

                EmitCommand(NaplpsCommandBuilder.BuildDomain((byte)prevSv, (byte)prevMv, 2));
                break;
            }

            default:
                Diag(DiagnosticSeverity.Error, w.Line, w.Column, $"'with {w.Attribute}' not supported");
                break;
        }
    }

    private void CompileRepeat(RepeatNode r)
    {
        var n = (int)Evaluate(r.Count);
        for (int i = 0; i < n; i++)
        {
            foreach (var s in r.Body) { CompileStatement(s); }
        }
    }

    private void CompileFor(ForNode f)
    {
        var from = (int)Evaluate(f.From);
        var to = (int)Evaluate(f.To);
        var previousValue = _scopeVars.TryGetValue(f.Variable, out var prev) ? (double?)prev : null;

        for (int i = from; i <= to; i++)
        {
            _scopeVars[f.Variable] = i;
            foreach (var s in f.Body) { CompileStatement(s); }
        }

        if (previousValue.HasValue)
        {
            _scopeVars[f.Variable] = previousValue.Value;
        }
        else
        {
            _scopeVars.Remove(f.Variable);
        }
    }

    private void CompileIf(IfNode i)
    {
        var cond = Evaluate(i.Condition);
        var body = cond != 0 ? i.Then : i.Else;

        if (body == null)
        {
            return;
        }

        foreach (var s in body) { CompileStatement(s); }
    }

    // ---- Proc calls (inline substitution) -----------------------------

    private void CompileProcCall(ProcCallNode call)
    {
        if (!_procs.TryGetValue(call.Name, out var proc))
        {
            Diag(DiagnosticSeverity.Error, call.Line, call.Column, $"Unknown procedure '{call.Name}'");
            return;
        }

        if (call.Args.Count != proc.Parameters.Count)
        {
            Diag(DiagnosticSeverity.Error, call.Line, call.Column, $"'{call.Name}' expects {proc.Parameters.Count} args, got {call.Args.Count}");
            return;
        }

        // Snapshot any outer let-bindings we're about to shadow, then reinstate them on exit.
        var shadowed = new Dictionary<string, double?>();

        for (int i = 0; i < proc.Parameters.Count; i++)
        {
            var name = proc.Parameters[i];
            shadowed[name] = _scopeVars.TryGetValue(name, out var prev) ? (double?)prev : null;
            _scopeVars[name] = Evaluate(call.Args[i]);
        }

        // @macro procs would wrap the body with DefMacro + End; for Phase 7 MVP we always
        // inline. The AsMacro flag stays on the node for future compile-as-macro support.
        foreach (var s in proc.Body) { CompileStatement(s); }

        foreach (var (name, prev) in shadowed)
        {
            if (prev.HasValue) { _scopeVars[name] = prev.Value; }
            else { _scopeVars.Remove(name); }
        }
    }

    // ---- Expression evaluator -----------------------------------------

    private double Evaluate(ExpressionNode expr)
    {
        switch (expr)
        {
            case NumberLiteralNode n:
                return n.Value;

            case FractionLiteralNode f:
                return f.AsDouble;

            case StringLiteralNode:
                Diag(DiagnosticSeverity.Error, expr.Line, expr.Column, "Cannot use a string in a numeric context");
                return 0;

            case IdentifierNode id:
                if (_scopeVars.TryGetValue(id.Name, out var v)) { return v; }
                if (_paletteAliases.TryGetValue(id.Name, out var p)) { return p; }
                Diag(DiagnosticSeverity.Error, id.Line, id.Column, $"Unknown identifier '{id.Name}'");
                return 0;

            case UnaryOpNode u:
                var operand = Evaluate(u.Operand);
                return u.Op switch { "-" => -operand, _ => operand };

            case BinaryOpNode b:
                var left = Evaluate(b.Left);
                var right = Evaluate(b.Right);
                return b.Op switch
                {
                    "+" => left + right,
                    "-" => left - right,
                    "*" => left * right,
                    "/" => right == 0 ? 0 : left / right,
                    "%" => right == 0 ? 0 : left % right,
                    _ => 0,
                };

            case CallExpressionNode:
                Diag(DiagnosticSeverity.Error, expr.Line, expr.Column, "Function-call expressions not yet evaluated at compile time");
                return 0;

            default:
                return 0;
        }
    }

    /// <summary>
    /// Emit raw opcode + operand bytes verbatim. First byte is the opcode; rest are operands.
    /// This is the lossless escape hatch: any NAPLPS byte sequence that the decompiler
    /// can't express in higher-level DSL gets round-tripped through raw.
    /// </summary>
    private void CompileRaw(RawStatementNode raw)
    {
        if (raw.Bytes.Count == 0)
        {
            return;
        }

        // Bypass AddCommand entirely — raw statements must NOT be instantiated through
        // InUseTable (which runs constructors that move the pen, change state, etc.).
        // Instead, create a bare NaplpsCommand and append it directly to the command list.
        // This preserves exact bytes without side effects.
        byte opcode = raw.Bytes[0];
        var operands = new NaplpsOperands();

        for (int i = 1; i < raw.Bytes.Count; i++)
        {
            operands.Add(raw.Bytes[i]);
        }

        var bareCmd = new NaplpsCommand(null, opcode, operands);
        _format.Commands.Add(new NaplpsSequence(new NaplpsState(), bareCmd));
    }

    // ---- Coord-mode projection ----------------------------------------

    private double NormX(double v) => _coordMode == CoordMode.Fractions ? v : v / _domainPixelWidth;
    private double NormY(double v) => _coordMode == CoordMode.Fractions ? v : v / _domainPixelHeight;
    private double NormW(double v) => _coordMode == CoordMode.Fractions ? v : v / _domainPixelWidth;
    private double NormH(double v) => _coordMode == CoordMode.Fractions ? v : v / _domainPixelHeight;

    // ---- Utilities -----------------------------------------------------

    private void EmitCommand((byte opcode, NaplpsOperands operands) cmd)
    {
        _format.AddCommand(cmd.opcode, cmd.operands);
    }

    private void ExpectArgs(CommandCallNode c, int minCount)
    {
        if (c.Args.Count < minCount)
        {
            Diag(DiagnosticSeverity.Error, c.Line, c.Column, $"'{c.Command}' needs at least {minCount} args, got {c.Args.Count}");
        }
    }

    private void Diag(DiagnosticSeverity severity, int line, int col, string message)
    {
        _diagnostics.Add(new Diagnostic(severity, line, col, message));
    }
}
