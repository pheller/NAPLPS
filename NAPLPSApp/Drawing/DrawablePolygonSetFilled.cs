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

namespace NAPLPSApp.Drawing;

public class DrawablePolygonSetFilled : IDrawable
{
    private PolygonSetFilledCommand _command;

    public DrawablePolygonSetFilled(PolygonSetFilledCommand command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, System.Drawing.Size size)
    {
        var polygonPoints = new List<PointF>();
        var startPoint = NaplpsUtils.ConvertNormalizedToPoint(size, _command.StartPoint.X, _command.StartPoint.Y);

        polygonPoints.Add(new PointF(startPoint.X, startPoint.Y));

        foreach (var drawPoint in _command.Points.Skip(1))
        {
            var polyPoint = NaplpsUtils.ConvertNormalizedToPoint(size, drawPoint.X, drawPoint.Y);

            polygonPoints.Add(new PointF(polyPoint.X, polyPoint.Y));
        }

        var color = state.Foreground.ToColor();
        var brush = Brushes.Solid(Color.FromRgba(color.R, color.G, color.B, color.A));

        image.Mutate(x => x.FillPolygon(brush, [.. polygonPoints]));
    }
}
