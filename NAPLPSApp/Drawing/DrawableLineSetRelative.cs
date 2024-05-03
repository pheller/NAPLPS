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

public class DrawableLineSetRelative : IDrawable
{
    private LineSetRelativeCommand _command;

    public DrawableLineSetRelative(LineSetRelativeCommand command)
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
        
        if (points.Count < 2)
        {
            return;
        }

        var lines = new SixLabors.ImageSharp.Drawing.Path(points.ToArray());

        var color = state.ColorMode == 0 ? state.Foreground.ToColor() : state.ColorMap[state.ColorMapForeground].ToColor();
        var pen = Pens.Solid(Color.FromRgba(color.R, color.G, color.B, color.A), 1f);

        image.Mutate(x => x.Draw(pen, lines));
    }
}
