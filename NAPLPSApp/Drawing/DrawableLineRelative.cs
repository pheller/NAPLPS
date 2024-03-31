using NAPLPS;
using NAPLPS.Commands;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = SixLabors.ImageSharp.Color;
using Pens = SixLabors.ImageSharp.Drawing.Processing.Pens;
using PointF = SixLabors.ImageSharp.PointF;

namespace NAPLPSApp.Drawing;

public class DrawableLineRelative : IDrawable
{
    private LineRelativeCommand _command;

    public DrawableLineRelative(LineRelativeCommand command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, System.Drawing.Size size)
    {
        var previousPenPoint = state.Pen;

        var startPoint = NaplpsUtils.ConvertNormalizedToPoint(size, previousPenPoint.X, previousPenPoint.Y);
        var endPoint = NaplpsUtils.ConvertNormalizedToPoint(size, previousPenPoint.X - _command.Point.X, previousPenPoint.Y - _command.Point.Y);

        var color = state.Foreground.ToColor();
        var pen = Pens.Solid(Color.FromRgba(color.R, color.G, color.B, color.A), 1f);

        image.Mutate(x => x.DrawLine(pen, [new PointF(startPoint.X, startPoint.Y), new PointF(endPoint.X, endPoint.Y)]));
    }
}
