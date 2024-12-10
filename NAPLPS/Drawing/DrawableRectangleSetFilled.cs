// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using PointF = SixLabors.ImageSharp.PointF;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace NAPLPS.Drawing;

public class DrawableRectangleSetFilled : Drawable, IDrawable
{
    private readonly RectangleSetFilledCommand _command;

    public DrawableRectangleSetFilled(RectangleSetFilledCommand command) : base(command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, Size size)
    {
        if (_command.StartPoint == Vector3.Zero)
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

                if (_command.ShouldFill)
                {
                    x.Fill(brush, rect);
                }

                if (!_command.ShouldFill || _command.Texture.ShouldHighlight)
                {
                    x.Draw(pen, rect);
                }
            }
        });
    }
}
