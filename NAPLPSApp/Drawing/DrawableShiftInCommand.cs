// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS;
using NAPLPS.Commands;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Reflection;
using Point = SixLabors.ImageSharp.Point;
using Color = SixLabors.ImageSharp.Color;
using FontFamily = SixLabors.Fonts.FontFamily;
using PointF = SixLabors.ImageSharp.PointF;

namespace NAPLPSApp.Drawing;

public class DrawableShiftInCommand : Drawable, IDrawable
{
    private readonly ShiftInCommand _command;

    public DrawableShiftInCommand(ShiftInCommand command) : base(command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, System.Drawing.Size size)
    {
        var (brush, pen) = GetBrushAndPenFromState();

        var points = new List<PointF>();

        var field = state.Field;
        var (x1, y1, x2, y2) = NaplpsUtils.ConvertRectToScreen(size, field.Origin.X, field.Origin.Y, field.Dimensions.X, field.Dimensions.Y);
        var point = new Point(x1, y1);

        var charSize = NaplpsUtils.ConvertNormalizedToPoint(size, state.TextFieldSize.X, state.TextFieldSize.Y);

        string text = _command.Text;
        // float TextPadding = 0f;
        // string TextFont = "Consolas";
        float TextFontSize = charSize.X;

        FontFamily fontFamily;

        FontCollection fontCollection = new();

        string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
        string fontPath = Path.Combine(appPath, "Fonts", "PRM5X10.TTF");

        fontFamily = fontCollection.Add(fontPath);

        var font = fontFamily.CreateFont(TextFontSize, SixLabors.Fonts.FontStyle.Regular);

        var options = new TextOptions(font)
        {
            Dpi = 92,
            KerningMode = KerningMode.Auto,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = SixLabors.Fonts.HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            HintingMode = HintingMode.None,
        };

        var fgcolor = state.ColorMode == 0 ? state.Foreground.ToColor() : state.ColorMap[state.ColorMapForeground].ToColor();
        var bgcolor = state.ColorMode == 0 ? state.Background.ToColor() : state.ColorMap[state.ColorMapBackground].ToColor();

        var drawingOptions = new DrawingOptions();

        image.Mutate(x =>
        {
            x.DrawText(drawingOptions, text, font, Color.FromRgba(bgcolor.R, bgcolor.G, bgcolor.B, bgcolor.A), new PointF(point.X, point.Y));
        });
    }
}
