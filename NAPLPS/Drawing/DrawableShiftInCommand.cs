// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System.Reflection;
using FontFamily = SixLabors.Fonts.FontFamily;
using PointF = SixLabors.ImageSharp.PointF;

namespace NAPLPS.Drawing;

public class DrawableShiftInCommand : Drawable, IDrawable
{
    private readonly ShiftInCommand _command;

    public DrawableShiftInCommand(ShiftInCommand command) : base(command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, Size size)
    {
        var points = new List<PointF>();

        var field = state.Field;
        var (x1, y1, x2, y2) = NaplpsUtils.ConvertRectToScreen(size, field.Origin.X, field.Origin.Y, field.Dimensions.X, field.Dimensions.Y);
        var point = new Point(x1, y1);
        var pointF = new PointF(point.X, point.Y);

        var (charSizeX, charSizeY) = ConvertNormalizedToScreenScale(size, state.CharSize.X, state.CharSize.Y);

        var text = "FIX ME";

        float TextFontSize = charSizeX;

        FontFamily fontFamily;

        FontCollection fontCollection = new();

        var appPath = IOPath.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";

        var fontPath = IOPath.Combine(appPath, "Fonts", "PRM5X10.TTF");

        fontFamily = fontCollection.Add(fontPath);

        var font = fontFamily.CreateFont(TextFontSize, FontStyle.Regular);

        var options = new TextOptions(font)
        {
            Dpi = 92,
            KerningMode = KerningMode.Auto,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            HintingMode = HintingMode.None,
        };

        var (fgColor, bgColor) = _command.GetColors(state);

        var drawingOptions = new DrawingOptions
        {
            Transform = Matrix3x2.CreateScale(1, charSizeY / charSizeX, pointF)
        };

        image.Mutate(x =>
        {
            x.DrawText(drawingOptions, text, font, fgColor.ToISColor(), pointF);

        });
    }
}
