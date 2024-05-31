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

public class DrawableRectangleSetFilled : IDrawable
{
    private readonly RectangleSetFilledCommand _command;

    public DrawableRectangleSetFilled(RectangleSetFilledCommand command)
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

        var normalizedStartPoint = _command.StartPoint;
        var normalizedDimensions = _command.Dimensions;

        // Adjust for negative height in normalized coordinates
        if (normalizedDimensions.Y < 0)
        {
            normalizedStartPoint.Y += normalizedDimensions.Y;  // Move start point up by the height
            normalizedDimensions.Y = Math.Abs(normalizedDimensions.Y);  // Use the absolute value of the height
        }

        // Adjust for negative width in normalized coordinates
        if (normalizedDimensions.X < 0)
        {
            normalizedStartPoint.X += normalizedDimensions.X;  // Move start point left by the width
            normalizedDimensions.X = Math.Abs(normalizedDimensions.X);  // Use the absolute value of the width
        }

        var normalizedEndpoint = normalizedStartPoint + normalizedDimensions;

        var startPoint = NaplpsUtils.ConvertNormalizedToPoint(size, normalizedStartPoint.X, normalizedStartPoint.Y);
        var dimensions = NaplpsUtils.ConvertNormalizedToPoint(size, normalizedEndpoint.X, normalizedEndpoint.Y);

        var rect = new RectangularPolygon(new PointF(startPoint.X, startPoint.Y), new PointF(dimensions.X, dimensions.Y));

        var (brush, pen) = GetBrushAndPenFromState(state);

        image.Mutate(x => x.Fill(brush, rect));

        //image.Mutate(x => x.Fill(brush, rect).Draw(pen, rect));

        //image.Mutate(x => x.Fill(brush, star2).Draw(pen, star2));
    }

    private static (SolidBrush, SolidPen) GetBrushAndPenFromState(NaplpsState state)
    {
        var fgcolor = state.ColorMode == 0 ? state.Foreground.ToColor() : state.ColorMap[state.ColorMapForeground].ToColor();
        var bgcolor = state.ColorMode == 0 ? state.Background.ToColor() : state.ColorMap[state.ColorMapBackground].ToColor();

        return (
            Brushes.Solid(Color.FromRgba(fgcolor.R, fgcolor.G, fgcolor.B, fgcolor.A)),
            Pens.Solid(Color.FromRgba(fgcolor.R, fgcolor.G, fgcolor.B, fgcolor.A), 1f)
        );
    }
}
