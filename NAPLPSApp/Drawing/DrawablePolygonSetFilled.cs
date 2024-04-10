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

        foreach (var drawPoint in _command.Points)
        {
            var polyPoint = NaplpsUtils.ConvertNormalizedToPoint(size, drawPoint.X, drawPoint.Y);

            polygonPoints.Add(new PointF(polyPoint.X, polyPoint.Y));
        }

        var polygon = new Polygon(polygonPoints.ToArray());

        var fgcolor = state.ColorMode == 0 ? state.Foreground.ToColor() : state.ColorMap[state.ColorMapForegroundSelected].ToColor();
        var bgcolor = state.ColorMode == 0 ? state.Background.ToColor() : state.ColorMap[state.ColorMapBackgroundSelected].ToColor();

        var pen = Pens.Solid(Color.FromRgba(fgcolor.R, fgcolor.G, fgcolor.B, fgcolor.A), 1f);
        var brush = Brushes.Solid(Color.FromRgba(fgcolor.R, fgcolor.G, fgcolor.B, fgcolor.A));

        image.Mutate(x => x.Fill(brush, polygon).Draw(pen, polygon));
    }
}
