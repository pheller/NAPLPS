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

    // Pre-computed reference measurements at size 100
    private static readonly float _refLineHeight;
    private static readonly float _refTopOffset;
    private static readonly float _refCharWidth;

    static DrawableAsciiChar()
    {
        var assembly = typeof(DrawableAsciiChar).Assembly;

        // Use PRM5X10 as the base font — it's the "standard" NAPLPS font
        using var stream = assembly.GetManifestResourceStream("NAPLPS.Fonts.PRM5X10.TTF");

        if (stream == null)
        {
            throw new InvalidOperationException("Could not load embedded font resource.");
        }

        _fontFamily = _fontCollection.Add(stream);

        // Measure reference bounds at size 100
        var refFont = _fontFamily.CreateFont(100f, FontStyle.Regular);
        var refBounds = TextMeasurer.MeasureBounds("Mg", new TextOptions(refFont));
        var refWidthBounds = TextMeasurer.MeasureBounds("M", new TextOptions(refFont));

        _refLineHeight = refBounds.Height;
        _refTopOffset = refBounds.Top;
        _refCharWidth = refWidthBounds.Width;
    }

    public DrawableAsciiChar(AsciiCharCommand command) : base(command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, Size size)
    {
        // Convert the pen (normalized NAPLPS coords) to screen pixel coordinates.
        var penPoint = ConvertNormalizedToPoint(size, state.Pen.X, state.Pen.Y);

        // Convert character cell size (normalized) to screen pixels
        var (charSizeX, charSizeY) = ConvertNormalizedToScreenScale(size, state.CharSize.X, state.CharSize.Y);

        float cellW = MathF.Max(1f, MathF.Abs(charSizeX));
        float cellH = MathF.Max(1f, MathF.Abs(charSizeY));

        // Cell top-left in screen coords (Y-down)
        float cellTopX = penPoint.X;
        float cellTopY = penPoint.Y - cellH;

        var rect = new RectangleF(cellTopX, cellTopY, cellW, cellH);

        var (fgColor, bgColor) = GetISColorFromState();

        // Use a fixed font size, then STRETCH it to fit the cell.
        // This mimics how old NAPLPS renderers worked — they'd blit a bitmap
        // font and stretch it to fit the character field dimensions.
        float fontSize = 100f;
        var font = _fontFamily.CreateFont(fontSize, FontStyle.Regular);

        // Calculate scale factors to stretch the glyph to fit the cell
        // Leave a small margin (90% fill) for authentic spacing
        float targetW = cellW * 0.85f;
        float targetH = cellH * 0.90f;

        float scaleX = targetW / _refCharWidth;
        float scaleY = targetH / _refLineHeight;

        // Scaled dimensions
        float scaledLineHeight = _refLineHeight * scaleY;
        float scaledTopOffset = _refTopOffset * scaleY;

        // Center vertically
        float vertPad = (cellH - scaledLineHeight) / 2f;

        // Position for the glyph's top to land at cellTopY + vertPad
        // But we need to account for the transform scaling around origin
        float textOriginY = (cellTopY + vertPad - scaledTopOffset) / scaleY;
        float textOriginX = cellTopX / scaleX;

        // Create transform that scales from origin (0,0)
        var transform = Matrix3x2.CreateScale(scaleX, scaleY);

        var drawingOptions = new DrawingOptions
        {
            Transform = transform
        };

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

                // ASCII code label
                var labelFontSize = MathF.Max(8f, MathF.Min(14f, cellW));
                var labelFont = _fontFamily.CreateFont(labelFontSize, FontStyle.Regular);
                var labelText = $"{(int)_command.AsciiCharacter}";
                ctx.DrawText(labelText, labelFont, fgColor, new PointF(cellTopX + 2, cellTopY + 2));

                // Baseline reference line
                float debugBaseline = cellTopY + vertPad + scaledLineHeight;
                ctx.DrawLine(debugDashedPen, new PointF(cellTopX, debugBaseline), new PointF(cellTopX + cellW, debugBaseline));
            }

            // Draw with stretch transform
            ctx.DrawText(drawingOptions, charText, font, fgColor, new PointF(textOriginX, textOriginY));
        });
    }
}
