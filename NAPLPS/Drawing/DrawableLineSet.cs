// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using Pens = SixLabors.ImageSharp.Drawing.Processing.Pens;
using PointF = SixLabors.ImageSharp.PointF;

namespace NAPLPS.Drawing;

public class DrawableLineSet : Drawable, IDrawable
{
    private readonly LineCommand _command;

    public DrawableLineSet(LineCommand command) : base(command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, Size size)
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

        float penWidth = GetPenWidth(size);

        var color = state.ColorMode == 0 ? state.Foreground.ToColor() : state.ColorMap[state.ColorMapForeground].ToColor();
        var pen = Pens.Solid(color.ToISColor(), penWidth);

        image.Mutate(x =>
        {
            for (var i = 0; i < points.Count - 1; i += 2)
            {
                var line = new SixLabors.ImageSharp.Drawing.Path(points.GetRange(i, 2).ToArray());
                x.Draw(pen, line);
            }
        });
    }
}
