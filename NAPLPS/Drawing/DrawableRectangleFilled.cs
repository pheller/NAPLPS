// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using PointF = SixLabors.ImageSharp.PointF;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using Pens = SixLabors.ImageSharp.Drawing.Processing.Pens;
using Brushes = SixLabors.ImageSharp.Drawing.Processing.Brushes;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace NAPLPS.Drawing;

public class DrawableRectangleFilled : IDrawable
{
    private readonly RectangleFilledCommand _command;

    public DrawableRectangleFilled(RectangleFilledCommand command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, Size size)
    {
        var polygonPoints = new List<PointF>();

        var startPointF = _command.Points.FirstOrDefault();

        var startPoint = NaplpsUtils.ConvertNormalizedToPoint(size, startPointF.X, startPointF.Y);

        var fgcolor = state.ColorMode == 0 ? state.Foreground.ToColor() : state.ColorMap[state.ColorMapForeground].ToColor();
        var bgcolor = state.ColorMode == 0 ? state.Background.ToColor() : state.ColorMap[state.ColorMapBackground].ToColor();

        var dimensions = NaplpsUtils.ConvertNormalizedToPoint(size, _command.Dimensions.X, _command.Dimensions.Y);

        // var rect = Rectangle.FromLTRB(startPoint.X, startPoint.Y + dimensions.Y,  )
        var rect = new Rectangle(startPoint.X, startPoint.Y, dimensions.X, dimensions.Y);

        var pen = Pens.Solid(fgcolor.ToISColor(), 1f);
        var brush = Brushes.Solid(fgcolor.ToISColor());

        image.Mutate(x => x.Fill(brush, rect).Draw(pen, rect));
    }
}
