// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using PointF = SixLabors.ImageSharp.PointF;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace NAPLPS.Drawing;

public class DrawablePolygon : Drawable, IDrawable
{
    private readonly PolygonCommand _command;

    public DrawablePolygon(PolygonCommand command) : base(command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, Size size)
    {
        var polygonPoints = new List<PointF>();

        foreach (var drawPoint in _command.Points)
        {
            var polyPoint = NaplpsUtils.ConvertNormalizedToPoint(size, drawPoint.X, drawPoint.Y);

            polygonPoints.Add(new PointF(polyPoint.X, polyPoint.Y));
        }

        if (polygonPoints.Count <= 1)
        {
            return;
        }

        var polygon = new Polygon(polygonPoints.ToArray());
        var (brush, pen) = GetBrushAndPenFromFillableCommand(size);

        image.Mutate(x =>
        {
            if (_command.ShouldFill)
            {
                x.Fill(brush, polygon);
            }

            if (!_command.ShouldFill || _command.Texture.ShouldHighlight)
            {
                x.Draw(pen, polygon);
            }
        });
    }
}
