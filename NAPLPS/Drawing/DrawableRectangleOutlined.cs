// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using PointF = SixLabors.ImageSharp.PointF;

namespace NAPLPS.Drawing;

public class DrawableRectangleOutlined : Drawable, IDrawable
{
    private readonly RectangleOutlinedCommand _command;

    public DrawableRectangleOutlined(RectangleOutlinedCommand command) : base(command)
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

        float outlineWidth = GetPenWidth(size);
        var outlinePen = GetTexturedPen(GetOutlineColor(), outlineWidth);

        image.Mutate(x => x.Draw(outlinePen, rect));
    }
}
