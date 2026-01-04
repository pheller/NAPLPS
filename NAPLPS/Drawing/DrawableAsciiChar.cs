// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System.Reflection;
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
        var appPath = IOPath.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
        var fontPath = IOPath.Combine(appPath, "Fonts", "PRM5X10.TTF");

        // Ensure we don't crash if font is missing, though rendering will fail
        if (File.Exists(fontPath))
        {
            _fontFamily = _fontCollection.Add(fontPath);
        }
        else
        {
            _fontFamily = SystemFonts.Get("Arial"); // Fallback
        }
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

        float w = MathF.Max(1f, MathF.Abs(charSizeX));
        float h = MathF.Max(1f, MathF.Abs(charSizeY));

        // Per NAPLPS spec: pen is the lower-left corner of the character field
        float drawX = penPoint.X;
        float drawY = penPoint.Y - h; // Shift box up so pen is bottom-left corner

        var rect = new RectangleF(drawX, drawY, w, h);

        var color = state.ColorMode == 0 ? state.Foreground : state.ColorMap[state.ColorMapForeground];
        var stroke = Pens.Solid(color.ToColor().ToISColor(), 1f);

        image.Mutate(ctx =>
        {
            ctx.Fill(new DrawingOptions(), ISColor.Black, rect);

            // Debug box
            ctx.Draw(stroke, rect);

            // Crosshair at pen origin (bottom-left of the box)
            ctx.DrawLine(stroke, new PointF(drawX - 4, penPoint.Y), new PointF(drawX + 4, penPoint.Y));
            ctx.DrawLine(stroke, new PointF(drawX, penPoint.Y - 4), new PointF(drawX, penPoint.Y + 4));

            // ASCII code label (optional debug text)
            var labelFontSize = MathF.Max(8f, MathF.Min(14f, w));
            var labelFont = _fontFamily.CreateFont(labelFontSize, FontStyle.Regular);
            var labelText = $"{(int)_command.AsciiCharacter}";
            ctx.DrawText(labelText, labelFont, color.ToColor().ToISColor(), new PointF(drawX + 2, drawY + 2));

        });
    }
}