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
        sb.AppendLine();

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

        return sb.ToString();
    }

    /// <summary>Decompile a .nap file on disk to a Telidraw source string.</summary>
    public static string DecompileFile(string napFilePath)
    {
        var format = NaplpsFormat.FromFile(napFilePath);
        return Decompile(format);
    }

    // ---- Per-command emission ------------------------------------------

    private static void EmitCommand(StringBuilder sb, NaplpsCommand cmd, NaplpsState stateBefore)
    {
        // Phase 8 byte-identity guarantee: EVERY command goes through raw to preserve
        // exact operand bytes. The trailing comment shows the human-readable command name
        // from the registry. High-level readable forms (move, line, rect, etc.) will
        // replace raw statements incrementally once each form is proven round-trip-safe.
        EmitRaw(sb, cmd);
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
