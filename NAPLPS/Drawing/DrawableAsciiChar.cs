// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using FontFamily = SixLabors.Fonts.FontFamily;
using PointF = SixLabors.ImageSharp.PointF;

namespace NAPLPS.Drawing;

public class DrawableAsciiChar : Drawable, IDrawable
{
    private readonly AsciiCharCommand _command;
    private static readonly FontCollection _fontCollection = new();
    private static readonly FontFamily _fontFamily;

    // Pre-computed from measuring "Mg" at reference size 100:
    // full glyph extent (top of M to bottom of g) and the Y offset from DrawText origin
    // to the top of the tallest glyph.
    private static readonly float _refLineHeight;
    private static readonly float _refTopOffset;

    static DrawableAsciiChar()
    {
        var assembly = typeof(DrawableAsciiChar).Assembly;

        using var stream = assembly.GetManifestResourceStream("NAPLPS.Fonts.PRM5X10.TTF");

        if (stream == null)
        {
            throw new InvalidOperationException("Could not load embedded font resource.");
        }

        _fontFamily = _fontCollection.Add(stream);

        // Measure the full glyph extent at a reference size.
        // "Mg" captures both the tallest ascender (M) and deepest descender (g).
        var refFont = _fontFamily.CreateFont(100f, FontStyle.Regular);
        var refBounds = TextMeasurer.MeasureBounds("Mg", new TextOptions(refFont));
        _refLineHeight = refBounds.Height;
        _refTopOffset = refBounds.Top;
    }

    public DrawableAsciiChar(AsciiCharCommand command) : base(command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, Size size)
    {
        // Convert the pen (normalized NAPLPS coords) to screen pixel coordinates.
        // ConvertNormalizedToPoint flips Y so penPoint is in screen coords (Y-down)
        // at the bottom-left of the character cell.
        var penPoint = ConvertNormalizedToPoint(size, state.Pen.X, state.Pen.Y);

        // Convert character cell size (normalized) to screen pixels
        var (charSizeX, charSizeY) = ConvertNormalizedToScreenScale(size, state.CharSize.X, state.CharSize.Y);

        float cellW = MathF.Max(1f, MathF.Abs(charSizeX));
        float cellH = MathF.Max(1f, MathF.Abs(charSizeY));

        // Cell top-left in screen coords (Y-down): pen is at bottom-left, top is above
        float cellTopX = penPoint.X;
        float cellTopY = penPoint.Y - cellH;

        var rect = new RectangleF(cellTopX, cellTopY, cellW, cellH);

        var (fgColor, bgColor) = GetISColorFromState();

        // Scale font so the full glyph range (ascender + descender) fits within the cell.
        // Leave a small margin for authentic NAPLPS pixel font spacing.
        float targetHeight = cellH * 0.90f;
        float fontSize = targetHeight * 100f / _refLineHeight;
        var font = _fontFamily.CreateFont(fontSize, FontStyle.Regular);

        // Position the text block consistently for ALL characters (shared baseline).
        // Scale the reference measurements to the current font size.
        float scale = fontSize / 100f;
        float scaledLineHeight = _refLineHeight * scale;
        float scaledTopOffset = _refTopOffset * scale;

        // Center the glyph block vertically within the cell
        float vertPad = (cellH - scaledLineHeight) / 2f;

        // The DrawText origin Y: we want glyphs' top (at textOriginY + scaledTopOffset)
        // to land at cellTopY + vertPad.
        float textOriginY = cellTopY + vertPad - scaledTopOffset;

        var charText = _command.AsciiCharacter.ToString();

        image.Mutate(ctx =>
        {
            ctx.Fill(new DrawingOptions(), bgColor, rect);
            ctx.Draw(Pens.Solid(bgColor, 1f), rect);

            if (Options.DebugTextDrawing)
            {
                var debugStrokePen = Pens.Solid(fgColor, 1f);
                var debugDashedPen = Pens.Dash(fgColor, 1f);

                // Crosshair at pen origin (bottom-left of the box)
                ctx.DrawLine(debugStrokePen, new PointF(cellTopX - 4, penPoint.Y), new PointF(cellTopX + 4, penPoint.Y));
                ctx.DrawLine(debugStrokePen, new PointF(cellTopX, penPoint.Y - 4), new PointF(cellTopX, penPoint.Y + 4));
                ctx.Draw(debugStrokePen, rect);

                // ASCII code label (optional debug text)
                var labelFontSize = MathF.Max(8f, MathF.Min(14f, cellW));
                var labelFont = _fontFamily.CreateFont(labelFontSize, FontStyle.Regular);
                var labelText = $"{(int)_command.AsciiCharacter}";

                ctx.DrawText(labelText, labelFont, fgColor, new PointF(cellTopX + 2, cellTopY + 2));

                // Baseline reference line (bottom of glyph block)
                float debugBaseline = cellTopY + vertPad + scaledLineHeight;
                ctx.DrawLine(debugDashedPen, new PointF(cellTopX, debugBaseline), new PointF(cellTopX + cellW, debugBaseline));
            }

            // Draw at left edge of cell — same textOriginY for all characters (shared baseline)
            ctx.DrawText(charText, font, fgColor, new PointF(cellTopX, textOriginY));
        });
    }
}
