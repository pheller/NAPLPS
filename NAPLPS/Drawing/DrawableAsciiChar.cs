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

    static DrawableAsciiChar()
    {
        var assembly = typeof(DrawableAsciiChar).Assembly;

        using var stream = assembly.GetManifestResourceStream("NAPLPS.Fonts.PRM5X10.TTF");

        if (stream == null)
        {
            throw new InvalidOperationException("Could not load embedded font resource.");
        }

        _fontFamily = _fontCollection.Add(stream);
    }

    public DrawableAsciiChar(AsciiCharCommand command) : base(command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, Size size)
    {
        // Convert the pen (normalized) to pixel coordinates
        var penPoint = ConvertNormalizedToPoint(size, state.Pen.X, state.Pen.Y);

        // Convert character cell size (normalized) to screen scale (pixels)
        var (charSizeX, charSizeY) = ConvertNormalizedToScreenScale(size, state.CharSize.X, state.CharSize.Y);

        float charWidth = MathF.Max(1f, MathF.Abs(charSizeX));
        float charHeight = MathF.Max(1f, MathF.Abs(charSizeY));

        // Per NAPLPS spec: pen is the lower-left corner of the character field
        float drawX = penPoint.X;
        float drawY = penPoint.Y - charHeight; // Shift box up so pen is bottom-left corner

        var pointF = new PointF(drawX, drawY);

        var rect = new RectangleF(drawX, drawY, charWidth, charHeight);

        var (fgColor, bgColor) = GetISColorFromState();

        var font = _fontFamily.CreateFont(charHeight * 0.80f, FontStyle.Regular);

        // --- NAPLPS baseline positioning (additive) ---
        // Baseline ~20% above bottom of field (per common NAPLPS guidance)
        var baselineOffset = charHeight * 0.20f;

        // Convert font metrics to a pixel ascender so we can place baseline more predictably.
        var metrics = font.FontMetrics;
        var ascenderPx = MathF.Abs(metrics.VerticalMetrics.Ascender / (float)metrics.UnitsPerEm * font.Size);

        var baselineY = drawY + charHeight - baselineOffset;

        var charText = _command.AsciiCharacter.ToString();

        // --- NAPLPS glyph width fit (additive) ---
        // Leave some margin in the character field for inter-character gap.
        // NAPLPS guidance: if you cannot leave both, leave space on the right.
        var horizontalPadding = MathF.Max(1f, charWidth * 0.10f);
        var targetGlyphWidth = MathF.Max(1f, charWidth - horizontalPadding);

        // Measure glyph width at current font size (uses ImageSharp text measurement).
        var measured = TextMeasurer.MeasureBounds(charText, new TextOptions(font));
        var measuredWidth = MathF.Max(1f, measured.Width / .85f);

        var leftBearing = measured.Left;

        var fitScaleX = MathF.Min(1f, targetGlyphWidth / measuredWidth);

        var glyphX = drawX - (leftBearing / fitScaleX);

        var naplpsTextPoint = new PointF(glyphX, baselineY - ascenderPx);

        // Scale around the center of the character field so shrink-fit stays centered.
        var fieldCenter = new PointF(drawX + (charWidth / 2f), drawY + (charHeight / 2f));

        var widthFitTransform = Matrix3x2.CreateScale((float)(fitScaleX), 1f, fieldCenter);

        var drawingOptions = new DrawingOptions
        {
            Transform = widthFitTransform
        };

        image.Mutate(ctx =>
        {
            ctx.Fill(new DrawingOptions(), bgColor, rect);
            ctx.Draw(Pens.Solid(bgColor, 1f), rect);

            if (Options.DebugTextDrawing)
            {
                var debugStrokePen = Pens.Solid(fgColor, 1f);
                var debugDashedPen = Pens.Dash(fgColor, 1f);

                // Crosshair at pen origin (bottom-left of the box)
                ctx.DrawLine(debugStrokePen, new PointF(drawX - 4, penPoint.Y), new PointF(drawX + 4, penPoint.Y));
                ctx.DrawLine(debugStrokePen, new PointF(drawX, penPoint.Y - 4), new PointF(drawX, penPoint.Y + 4));
                ctx.Draw(debugStrokePen, rect);

                // ASCII code label (optional debug text)
                var labelFontSize = MathF.Max(8f, MathF.Min(14f, charWidth));
                var labelFont = _fontFamily.CreateFont(labelFontSize, FontStyle.Regular);
                var labelText = $"{(int)_command.AsciiCharacter}";

                ctx.DrawText(labelText, labelFont, fgColor, new PointF(drawX + 2, drawY + 2));

                ctx.DrawLine(debugDashedPen, new PointF(drawX, baselineY), new PointF(drawX + charWidth, baselineY));
            }

            ctx.DrawText(drawingOptions, charText, font, fgColor, naplpsTextPoint);
        });
    }
}