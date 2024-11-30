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
using SixLabors.ImageSharp.Drawing;

namespace NAPLPSApp.Drawing;

public class DrawablePolygon : Drawable, IDrawable
{
    private readonly PolygonCommand _command;

    public DrawablePolygon(PolygonCommand command) : base(command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, System.Drawing.Size size)
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
        var (brush, pen) = GetBrushAndPenFromFillableCommand();

        image.Mutate(x => {
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
