// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using NAPLPS.Drawing;

namespace NAPLPS;

/// <summary>
/// Compares two NAPLPS files structurally (command-level) and visually (rendered diff).
/// </summary>
public static class NapDiff
{
    public record DiffEntry(int? IndexA, int? IndexB, string CommandA, string CommandB, bool IsDifferent);

    /// <summary>
    /// Compares commands between two NAPLPS files, producing a line-by-line diff.
    /// Uses a simple parallel walk (not LCS) — sufficient for comparing file versions.
    /// </summary>
    public static List<DiffEntry> CommandDiff(NaplpsFormat a, NaplpsFormat b)
    {
        var result = new List<DiffEntry>();
        int maxLen = Math.Max(a.Commands.Count, b.Commands.Count);

        for (int i = 0; i < maxLen; i++)
        {
            string cmdA = i < a.Commands.Count ? FormatCommand(a.Commands[i].Command) : "";
            string cmdB = i < b.Commands.Count ? FormatCommand(b.Commands[i].Command) : "";
            bool isDifferent = cmdA != cmdB;

            result.Add(new DiffEntry(
                i < a.Commands.Count ? i : null,
                i < b.Commands.Count ? i : null,
                cmdA,
                cmdB,
                isDifferent));
        }

        return result;
    }

    /// <summary>
    /// Renders both files and creates a side-by-side composite image with diff highlighting.
    /// Differing pixels are highlighted in magenta.
    /// </summary>
    public static Image<Rgba32> VisualDiff(NaplpsFormat a, NaplpsFormat b, Size size)
    {
        using var ctxA = new DrawContext(a, size);
        using var ctxB = new DrawContext(b, size);
        ctxA.Render();
        ctxB.Render();

        int dividerWidth = 2;
        var composite = new Image<Rgba32>(size.Width * 2 + dividerWidth, size.Height);

        // Copy image A to left side
        composite.ProcessPixelRows(ctxA.Image, (dst, src) =>
        {
            for (int y = 0; y < src.Height; y++)
            {
                var srcRow = src.GetRowSpan(y);
                var dstRow = dst.GetRowSpan(y);
                srcRow.CopyTo(dstRow);
            }
        });

        // Draw divider
        composite.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = size.Width; x < size.Width + dividerWidth; x++)
                {
                    row[x] = new Rgba32(128, 128, 128, 255);
                }
            }
        });

        // Copy image B to right side, highlighting differences
        int rightOffset = size.Width + dividerWidth;
        composite.ProcessPixelRows(ctxA.Image, ctxB.Image, (dst, srcA, srcB) =>
        {
            for (int y = 0; y < srcB.Height; y++)
            {
                var dstRow = dst.GetRowSpan(y);
                var rowA = srcA.GetRowSpan(y);
                var rowB = srcB.GetRowSpan(y);

                for (int x = 0; x < srcB.Width; x++)
                {
                    if (rowA[x] != rowB[x])
                    {
                        // Highlight difference in magenta
                        dstRow[rightOffset + x] = new Rgba32(255, 0, 255, 255);
                    }
                    else
                    {
                        dstRow[rightOffset + x] = rowB[x];
                    }
                }
            }
        });

        return composite;
    }

    private static string FormatCommand(NaplpsCommand cmd)
    {
        var ops = cmd.Operands.Count > 0
            ? " [" + string.Join(",", cmd.Operands.Select(b => $"0x{b:X2}")) + "]"
            : "";
        return $"0x{cmd.OpCode:X2} {cmd.GetType().Name}{ops}";
    }
}
