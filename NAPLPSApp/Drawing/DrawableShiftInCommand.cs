// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS;
using NAPLPS.Commands;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Brushes = SixLabors.ImageSharp.Drawing.Processing.Brushes;
using Color = SixLabors.ImageSharp.Color;
using FontFamily = SixLabors.Fonts.FontFamily;
using PointF = SixLabors.ImageSharp.PointF;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace NAPLPSApp.Drawing;

public class DrawableShiftInCommand : IDrawable
{
    private readonly ShiftInCommand _command;

    public DrawableShiftInCommand(ShiftInCommand command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, System.Drawing.Size size)
    {
        var points = new List<PointF>();

        var point = NaplpsUtils.ConvertNormalizedToPoint(size, state.Pen.X, state.Pen.Y);

        var charSize = NaplpsUtils.ConvertNormalizedToPoint(size, state.TextFieldSize.X, state.TextFieldSize.Y);

        string text = _command.Text;
        // float TextPadding = 0f;
        // string TextFont = "Consolas";
        float TextFontSize = charSize.X;

        FontFamily fontFamily;

        FontCollection fontCollection = new();

        fontFamily = fontCollection.Add("Fonts\\PRM5X10.TTF");

        var font = fontFamily.CreateFont(TextFontSize, SixLabors.Fonts.FontStyle.Regular);

        var options = new TextOptions(font)
        {
            Dpi = 92,
            KerningMode = KerningMode.Auto
        };

        var rectF = TextMeasurer.MeasureBounds(text, options);

        var rect = new Rectangle(point.X, point.Y - (int)rectF.Height, (int)rectF.Width, (int)rectF.Height);

        var fgcolor = state.ColorMode == 0 ? state.Foreground.ToColor() : state.ColorMap[state.ColorMapForeground].ToColor();
        var bgcolor = state.ColorMode == 0 ? state.Background.ToColor() : state.ColorMap[state.ColorMapBackground].ToColor();

        var brush = Brushes.Solid(Color.FromRgba(bgcolor.R, bgcolor.G, bgcolor.B, bgcolor.A));

        image.Mutate(x => x.Fill(brush, rect));
        image.Mutate(x => x.DrawText(text, font, Color.FromRgba(fgcolor.R, fgcolor.G, fgcolor.B, fgcolor.A), new PointF(point.X, point.Y - rectF.Height)));
    }
}
