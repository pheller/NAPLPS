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
using SolidBrush = SixLabors.ImageSharp.Drawing.Processing.SolidBrush;
using Rectangle = SixLabors.ImageSharp.Rectangle;
using SixLabors.ImageSharp.Drawing;
using System.Numerics;

namespace NAPLPSApp.Drawing;

public class DrawableRectangleSetFilled : Drawable, IDrawable
{
    private readonly RectangleSetFilledCommand _command;

    public DrawableRectangleSetFilled(RectangleSetFilledCommand command) : base(command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, System.Drawing.Size size)
    {
        var polygonPoints = new List<PointF>();

        if (_command.StartPoint == Vector3.Zero)
        {
            return;
        }

        var startPoint = _command.StartPoint;
        var dimensions = _command.Dimensions;
        var (x1, y1, x2, y2) = NaplpsUtils.ConvertRectToScreen(size, startPoint.X, startPoint.Y, dimensions.X, dimensions.Y);

        var rect = new RectangularPolygon(new PointF(x1, y1), new PointF(x2, y2));
        var (brush, pen) = GetBrushAndPenFromFillableCommand();

        image.Mutate(x => {
            if (_command.ShouldFill)
            {
                x.Fill(brush, rect);
            }

            if (!_command.ShouldFill || _command.Texture.ShouldHighlight)
            {
                x.Draw(pen, rect);
            }
        });

        //image.Mutate(x => x.Fill(brush, rect).Draw(pen, rect));

        //image.Mutate(x => x.Fill(brush, star2).Draw(pen, star2));
    }
}
