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
using SixLabors.ImageSharp.Drawing;

namespace NAPLPSApp.Drawing;

public class DrawablePolygonOutlined : IDrawable
{
    private PolygonOutlinedCommand _command;

    public DrawablePolygonOutlined(PolygonOutlinedCommand command)
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

        var pen = Pens.Solid(Color.FromRgba(fgcolor.R, fgcolor.G, fgcolor.B, fgcolor.A), 1f);

        image.Mutate(x => x.Draw(pen, polygon));
    }
}
