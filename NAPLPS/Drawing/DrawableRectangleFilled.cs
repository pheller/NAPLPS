// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using PointF = SixLabors.ImageSharp.PointF;

namespace NAPLPS.Drawing;

public class DrawableRectangleFilled : Drawable, IDrawable
{
    private readonly RectangleFilledCommand _command;

    public DrawableRectangleFilled(RectangleFilledCommand command) : base(command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, Size size)
    {
        var startPoint = _command.Points.FirstOrDefault();
        var dimensions = _command.Dimensions;

        var (x1, y1, x2, y2) = NaplpsUtils.ConvertRectToScreen(
            size,
            startPoint.X,
            startPoint.Y,
            dimensions.X,
            dimensions.Y
        );

        var (brush, pen) = GetBrushAndPenFromFillableCommand(size);
        var rect = new RectangularPolygon(new PointF(x1, y1), new PointF(x2, y2));

        image.Mutate(x =>
        {
            if (_command.ShouldFill)
            {
                x.Fill(brush, rect);
            }

            if (!_command.ShouldFill || _command.Texture.ShouldHighlight)
            {
                float outlineWidth = GetPenWidth(size);
                x.Draw(Pens.Solid(GetOutlineColor(), outlineWidth), rect);
            }
        });
    }
}
