// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using PointF = SixLabors.ImageSharp.PointF;

namespace NAPLPS.Drawing;

public class DrawableRectangleSetOutlined : Drawable, IDrawable
{
    private readonly RectangleSetOutlinedCommand _command;

    public DrawableRectangleSetOutlined(RectangleSetOutlinedCommand command) : base(command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, Size size)
    {
        if (!_command.IsValid || _command.Vertices.Count < 2)
        {
            return;
        }

        var (brush, pen) = GetBrushAndPenFromFillableCommand(size);
        var vertices = _command.Vertices;

        image.Mutate(x =>
        {
            for (int i = 0; i < vertices.Count - 1; i += 2)
            {
                var v1 = vertices[i];
                var v2 = vertices[i + 1];

                var (x1, y1, x2, y2) = NaplpsUtils.ConvertRectToScreen(size, v1.X, v1.Y, v2.X, v2.Y);
                var rect = new RectangularPolygon(new PointF(x1, y1), new PointF(x2, y2));

                x.Draw(pen, rect);
            }
        });
    }
}
