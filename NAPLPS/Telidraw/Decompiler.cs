// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Globalization;
using System.Text;

namespace NAPLPS.Telidraw;

/// <summary>
/// Walks a <see cref="NaplpsFormat"/>'s command stream and emits flat Telidraw source
/// text. Per D2, no attempt is made to reconstruct <c>with</c> blocks, loops, or procs —
/// those are authoring constructs that don't exist in the NAPLPS byte stream. The output
/// is a linear sequence of command calls that, when fed back through the <see cref="Compiler"/>,
/// produces byte-identical NAPLPS output.
///
/// The <see cref="NaplpsFormat.New"/> header (CAN + NSR sentinels) is recognized and skipped
/// because the compiler re-emits it automatically. This makes decompile→compile→decompile
/// stable for any compiler-produced file.
/// </summary>
public static class Decompiler
{
    /// <summary>Decompile an in-memory NaplpsFormat to a Telidraw source string.</summary>
    public static string Decompile(NaplpsFormat format)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// Decompiled from .nap by Telidraw decompiler");
        sb.AppendLine("#coord fractions");

        // Emit a #bits directive so the compiler round-trips the original file's
        // numerical-data base (0x40 for 7-bit, 0xC0 for 8-bit). Without this, high-bit
        // differences break byte-for-byte equality even though the low 6 data bits match.
        sb.AppendLine($"#bits {(format.Is7Bit ? 7 : 8)}");
        sb.AppendLine();

        // Set encoder bit-width to match source file for the duration of this decompile.
        // The verifier calls the compiler per-candidate; compiled bytes must match the
        // original file's bit mode so high-bit comparison succeeds.
        var priorMode = NaplpsEncoder.Use7BitMode;
        NaplpsEncoder.Use7BitMode = format.Is7Bit;

        bool inTextRun = false;
        var textBuffer = new StringBuilder();

        for (int i = 0; i < format.Commands.Count; i++)
        {
            var seq = format.Commands[i];
            var cmd = seq.Command;

            // Group basic ASCII text characters into a single text "..." statement.
            // ONLY when the opcode byte IS the character code (Primary Character Set in GL).
            // Supplementary chars may have ASCII-range Unicode values (e.g., '/' at opcode 0x49)
            // that would produce wrong bytes if grouped — those go through raw.
            if (cmd is AsciiCharCommand ascii && cmd.OpCode == (byte)ascii.AsciiCharacter && ascii.AsciiCharacter >= 0x20 && ascii.AsciiCharacter <= 0x7E)
            {
                if (!inTextRun)
                {
                    inTextRun = true;
                    textBuffer.Clear();
                }
                textBuffer.Append(ascii.AsciiCharacter);
                continue;
            }

            // Flush any pending text run before the next non-text command.
            if (inTextRun)
            {
                sb.Append("text \"").Append(EscapeString(textBuffer.ToString())).AppendLine("\"");
                inTextRun = false;
            }

            EmitCommand(sb, cmd, seq.State);
        }

        // Flush final text run.
        if (inTextRun)
        {
            sb.Append("text \"").Append(EscapeString(textBuffer.ToString())).AppendLine("\"");
        }

        // Restore prior bit mode so nested/concurrent decompiles don't bleed state.
        NaplpsEncoder.Use7BitMode = priorMode;

        return sb.ToString();
    }

    /// <summary>Decompile a .nap file on disk to a Telidraw source string.</summary>
    public static string DecompileFile(string napFilePath)
    {
        var format = NaplpsFormat.FromFile(napFilePath);
        return Decompile(format);
    }

    // ---- Per-command emission ------------------------------------------

    /// <summary>
    /// Try every high-level form for this command in priority order. For each candidate,
    /// verify byte-identity by recompiling the proposed Telidraw line and comparing the
    /// emitted opcode+operands to the original command. The first form that round-trips
    /// wins. Falls back to <see cref="EmitRaw"/> when no high-level form matches —
    /// guaranteeing 100% byte fidelity while producing readable output everywhere it's safe.
    /// </summary>
    private static void EmitCommand(StringBuilder sb, NaplpsCommand cmd, NaplpsState stateBefore)
    {
        foreach (var candidate in HighLevelCandidates(cmd, stateBefore))
        {
            if (RoundTripsExactly(candidate, cmd))
            {
                sb.AppendLine(candidate);
                return;
            }
        }

        EmitRaw(sb, cmd);
    }

    /// <summary>
    /// Generate plausible high-level Telidraw lines for a given NAPLPS command. Each
    /// returned string must be a complete one-line statement. Order matters — return the
    /// most readable form first; the verifier picks the first one that round-trips.
    /// </summary>
    private static IEnumerable<string> HighLevelCandidates(NaplpsCommand cmd, NaplpsState stateBefore)
    {
        switch (cmd)
        {
            // Geometric — decode the operand bytes directly so we never reinvent coordinates.
            case PointSetAbsoluteCommand:
            {
                var (x, y) = NaplpsEncoder.DecodeVertex2D(cmd.Operands);
                yield return $"move {Fmt(x)} {Fmt(y)}";
                break;
            }

            case PointAbsoluteCommand:
            {
                var (x, y) = NaplpsEncoder.DecodeVertex2D(cmd.Operands);
                yield return $"point {Fmt(x)} {Fmt(y)}";
                break;
            }

            case PointSetRelativeCommand:
            {
                var (dx, dy) = NaplpsEncoder.DecodeVertex2D(cmd.Operands);
                yield return $"move-rel {Fmt(dx)} {Fmt(dy)}";
                break;
            }

            case PointRelativeCommand:
            {
                var (dx, dy) = NaplpsEncoder.DecodeVertex2D(cmd.Operands);
                yield return $"point-rel {Fmt(dx)} {Fmt(dy)}";
                break;
            }

            case LineAbsoluteCommand:
            {
                var (x, y) = NaplpsEncoder.DecodeVertex2D(cmd.Operands);
                yield return $"line {Fmt(x)} {Fmt(y)}";
                break;
            }

            case LineRelativeCommand:
            {
                var (dx, dy) = NaplpsEncoder.DecodeVertex2D(cmd.Operands);
                yield return $"line-rel {Fmt(dx)} {Fmt(dy)}";
                break;
            }

            case RectangleFilledCommand:
            {
                var (w, h) = NaplpsEncoder.DecodeVertex2D(cmd.Operands);
                yield return $"rect {Fmt(w)} {Fmt(h)}";
                break;
            }

            case RectangleOutlinedCommand:
            {
                var (w, h) = NaplpsEncoder.DecodeVertex2D(cmd.Operands);
                yield return $"rect-outline {Fmt(w)} {Fmt(h)}";
                break;
            }

            case ResetCommand:
            {
                yield return "reset";
                break;
            }

            case NonSelectiveResetCommand:
            {
                yield return "nsr";
                break;
            }

            case WaitCommand wc when wc.IsValid:
            {
                yield return $"wait {wc.WaitTime}";
                break;
            }

            case IncrementalFieldCommand ifc when ifc.Operands.Count == 0:
            {
                yield return "field";
                break;
            }

            // SELECT COLOR — palette index lives in bits 3-6 of the first operand byte.
            // For mode 2 (two operand groups, single-byte width = 1) propose `color fg bg` first;
            // mode 1 falls through to `color fg`. Verifier picks whichever round-trips.
            case SelectColorCommand sc when sc.Operands.Count >= 1:
            {
                int svBytes = stateBefore.SingleByteValue;
                int values = sc.Operands.Count / Math.Max(1, svBytes);
                byte fg = NaplpsUtils.ConvertBitsToByte([sc.Operands[0, 3], sc.Operands[0, 4], sc.Operands[0, 5], sc.Operands[0, 6]]);

                if (values >= 2 && svBytes == 1 && sc.Operands.Count >= 2)
                {
                    byte bg = NaplpsUtils.ConvertBitsToByte([sc.Operands[1, 3], sc.Operands[1, 4], sc.Operands[1, 5], sc.Operands[1, 6]]);
                    yield return $"color {fg} {bg}";
                }
                yield return $"color {fg}";
                break;
            }

            // DOMAIN — fixed byte packs (singleByte, multiByte, dimensionality).
            case DomainCommand dc when dc.Operands.Count >= 1:
            {
                var (sv, mv, dim) = DomainCommand.ProcessFixedByte(dc.Operands);
                yield return $"domain {sv} {mv} {dim}";
                break;
            }

            // TEXTURE — first operand byte packs (linePattern, highlight, fillPattern).
            case TextureCommand tc when tc.Operands.Count >= 1:
            {
                yield return $"texture {(byte)tc.LineTexture} {(tc.ShouldHighlight ? 1 : 0)} {(byte)tc.TexturePattern}";
                break;
            }
        }
    }

    /// <summary>
    /// Compile a candidate Telidraw line in isolation and verify the resulting opcode +
    /// operand bytes match the original command. Comparison is on the lower 6 data bits
    /// of each operand byte — the only bits the NAPLPS parser reads — so a 7-bit source
    /// (0x40-0x7F operand bytes) and the compiler's 8-bit output (0xC0-0xFF) compare equal
    /// when their data nibbles match. The raw-byte file-level round-trip test still proves
    /// EXACT byte preservation; this verifier guarantees SEMANTIC equivalence per command.
    /// </summary>
    private static bool RoundTripsExactly(string candidateLine, NaplpsCommand original)
    {
        try
        {
            var tokens = new Lexer(candidateLine).Tokenize();
            var parser = new Parser(tokens);
            var ast = parser.Parse();

            if (parser.Diagnostics.Count > 0)
            {
                return false;
            }

            var compiler = new Compiler(ast) { BareFormat = true };
            var format = compiler.Compile();

            if (compiler.Diagnostics.Count > 0 || format.Commands.Count != 1)
            {
                return false;
            }

            var emitted = format.Commands[0].Command;

            if (emitted.OpCode != original.OpCode || emitted.Operands.Count != original.Operands.Count)
            {
                return false;
            }

            for (int i = 0; i < emitted.Operands.Count; i++)
            {
                if (emitted.Operands[i] != original.Operands[i])
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Universal lossless fallback: emit `raw OPCODE OP1 OP2 ...` with a trailing comment
    /// showing the command name for readability. The compiler's CompileRaw passes the
    /// opcode + operand bytes through verbatim, guaranteeing byte-identity round-trip.
    /// </summary>
    private static void EmitRaw(StringBuilder sb, NaplpsCommand cmd)
    {
        sb.Append("raw ").Append(cmd.OpCode);

        foreach (var b in cmd.Operands)
        {
            sb.Append(' ').Append(b);
        }

        // Annotate with the command class name for human readability.
        var name = CommandRegistry.GetByType(cmd.GetType())?.Name ?? cmd.GetType().Name.Replace("Command", "");
        sb.Append("  // ").AppendLine(name);
    }

    // ---- Formatting helpers -------------------------------------------

    /// <summary>
    /// Format a float for Telidraw source. Uses invariant culture, strips trailing zeros,
    /// and snaps to common NAPLPS fractions (1/40, 5/128, etc.) when exact.
    /// </summary>
    private static string Fmt(float value)
    {
        // Common NAPLPS fractions and their decimal equivalents (within 9-bit precision).
        // If the value matches one of these, emit the fraction form for readability.
        if (TryFormatAsFraction(value, 40, out var frac)) { return frac; }
        if (TryFormatAsFraction(value, 128, out frac)) { return frac; }
        if (TryFormatAsFraction(value, 64, out frac)) { return frac; }
        if (TryFormatAsFraction(value, 32, out frac)) { return frac; }
        if (TryFormatAsFraction(value, 80, out frac)) { return frac; }

        // Fall back to decimal. Use enough precision to round-trip through EncodeVertex2D.
        return value.ToString("0.####", CultureInfo.InvariantCulture);
    }

    private static bool TryFormatAsFraction(float value, int denominator, out string result)
    {
        float numerator = value * denominator;
        int intNumerator = (int)MathF.Round(numerator);

        if (MathF.Abs(numerator - intNumerator) < 0.01f && intNumerator != 0)
        {
            result = $"{intNumerator}/{denominator}";
            return true;
        }

        result = string.Empty;
        return false;
    }

    private static string EscapeString(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
    }
}
