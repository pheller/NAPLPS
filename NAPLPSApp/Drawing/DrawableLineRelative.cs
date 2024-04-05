// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

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
        var points = new List<PointF>();

        foreach (var verts in _command.Points)
        {
            var thePoint = NaplpsUtils.ConvertNormalizedToPoint(size, verts.X, verts.Y);

            points.Add(new PointF(thePoint.X, thePoint.Y));
        }
        
        if (points.Count == 0)
        {
            return;
        }

        var lines = new SixLabors.ImageSharp.Drawing.Path(points.ToArray());

        var color = state.ColorMode == 0 ? state.Foreground.ToColor() : state.ColorMap[state.ColorMapForegroundSelected].ToColor();
        var pen = Pens.Solid(Color.FromRgba(color.R, color.G, color.B, color.A), 1f);

        image.Mutate(x => x.Draw(pen, lines));

        //var previousPenPoint = _command.Points.First();

        //var startPoint = NaplpsUtils.ConvertNormalizedToPoint(size, previousPenPoint.X, previousPenPoint.Y);
        //var endPoint = NaplpsUtils.ConvertNormalizedToPoint(size, previousPenPoint.X + _command.Point.X, previousPenPoint.Y + _command.Point.Y);

        //var color = state.ColorMode == 0 ? state.Foreground.ToColor() : state.ColorMap[state.ColorMapForegroundSelected].ToColor();
        //var pen = Pens.Solid(Color.FromRgba(color.R, color.G, color.B, color.A), 1f);

        //image.Mutate(x => x.DrawLine(pen, [new PointF(startPoint.X, startPoint.Y), new PointF(endPoint.X, endPoint.Y)]));
    }
}
