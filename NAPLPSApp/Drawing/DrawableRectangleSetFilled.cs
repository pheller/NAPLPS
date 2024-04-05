// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS;
using NAPLPS.Commands;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using Color = SixLabors.ImageSharp.Color;
using PointF = SixLabors.ImageSharp.PointF;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using Pens = SixLabors.ImageSharp.Drawing.Processing.Pens;
using Brushes = SixLabors.ImageSharp.Drawing.Processing.Brushes;
using System.CommandLine;
using SixLabors.ImageSharp.Drawing;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace NAPLPSApp.Drawing;

public class DrawableRectangleSetFilled : IDrawable
{
    private RectangleSetFilledCommand _command;

    public DrawableRectangleSetFilled(RectangleSetFilledCommand command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, System.Drawing.Size size)
    {
        var polygonPoints = new List<PointF>();

        var startPoint = NaplpsUtils.ConvertNormalizedToPoint(size, _command.StartPoint.X, _command.StartPoint.Y);

        var fgcolor = state.ColorMode == 0 ? state.Foreground.ToColor() : state.ColorMap[state.ColorMapForegroundSelected].ToColor();
        var bgcolor = state.ColorMode == 0 ? state.Background.ToColor() : state.ColorMap[state.ColorMapBackgroundSelected].ToColor();

        var dimensions = NaplpsUtils.ConvertNormalizedToPoint(size, _command.Dimensions.X, _command.Dimensions.Y);

        var rect = new Rectangle(startPoint.X, startPoint.Y - dimensions.Y, dimensions.X, dimensions.Y);

        var pen = Pens.Solid(Color.FromRgba(fgcolor.R, fgcolor.G, fgcolor.B, fgcolor.A), 1f);
        var brush = Brushes.Solid(Color.FromRgba(fgcolor.R, fgcolor.G, fgcolor.B, fgcolor.A));

        image.Mutate(x => x.Fill(brush, rect).Draw(pen, rect));
    }
}
