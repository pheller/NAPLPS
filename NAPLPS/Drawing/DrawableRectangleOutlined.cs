// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using PointF = SixLabors.ImageSharp.PointF;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using Pens = SixLabors.ImageSharp.Drawing.Processing.Pens;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace NAPLPS.Drawing;

public class DrawableRectangleOutlined : IDrawable
{
    private readonly RectangleOutlinedCommand _command;

    public DrawableRectangleOutlined(RectangleOutlinedCommand command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, Size size)
    {
        var polygonPoints = new List<PointF>();

        var startPointF = _command.Points.FirstOrDefault();

        var startPoint = NaplpsUtils.ConvertNormalizedToPoint(size, startPointF.X, startPointF.Y);

        var fgcolor = state.ColorMode == 0 ? state.Foreground.ToColor() : state.ColorMap[state.ColorMapForeground].ToColor();

        var dimensions = NaplpsUtils.ConvertNormalizedToPoint(size, _command.Dimensions.X, _command.Dimensions.Y);

        var rect = new Rectangle(startPoint.X, startPoint.Y, dimensions.X, dimensions.Y);

        var pen = Pens.Solid(fgcolor.ToISColor(), 1f);

        image.Mutate(x => x.Draw(pen, rect));
    }
}
