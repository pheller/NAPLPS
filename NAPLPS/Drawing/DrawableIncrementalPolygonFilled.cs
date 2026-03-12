// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using PointF = SixLabors.ImageSharp.PointF;

namespace NAPLPS.Drawing;

/// <summary>
/// Renders INCREMENTAL POLY FILLED commands.
/// Similar to incremental line but the shape is filled.
/// </summary>
public class DrawableIncrementalPolygonFilled : Drawable, IDrawable
{
    private readonly IncrementalPolygonFilledCommand _command;

    public DrawableIncrementalPolygonFilled(IncrementalPolygonFilledCommand command) : base(command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, Size size)
    {
        if (!_command.IsValid || _command.Segments.Count == 0)
        {
            return;
        }

        // Get starting position from current pen + offset
        var startPos = state.Pen;
        startPos.X += _command.StartOffset.X;
        startPos.Y += _command.StartOffset.Y;

        var startPoint = ConvertNormalizedToPoint(size, startPos.X, startPos.Y);
        float currentX = startPoint.X;
        float currentY = startPoint.Y;

        // Get logical pel size for scaling motion deltas
        var (pelWidth, pelHeight) = ConvertNormalizedToScreenScale(size, state.LogicalPel.X, state.LogicalPel.Y);
        pelWidth = Math.Max(1, Math.Abs(pelWidth));
        pelHeight = Math.Max(1, Math.Abs(pelHeight));

        // Build the polygon points
        var points = new List<PointF> { new PointF(currentX, currentY) };
        float lastKnownX = currentX;
        float lastKnownY = currentY;

        foreach (var segment in _command.Segments)
        {
            if (segment.ReturnToLastKnown)
            {
                // Return to last known position before continuing
                currentX = lastKnownX;
                currentY = lastKnownY;
                points.Add(new PointF(currentX, currentY));
            }

            // Calculate the next point
            float dx = segment.HasDx ? segment.Dx * pelWidth : 0;
            float dy = segment.HasDy ? segment.Dy * pelHeight : 0;

            float nextX = currentX + dx;
            float nextY = currentY - dy;

            points.Add(new PointF(nextX, nextY));

            // Update last known position when drawing
            if (segment.Draw)
            {
                lastKnownX = nextX;
                lastKnownY = nextY;
            }

            currentX = nextX;
            currentY = nextY;
        }

        // Close the polygon by returning to start if needed
        if (points.Count > 2)
        {
            var firstPoint = points[0];
            var lastPoint = points[^1];
            if (Math.Abs(firstPoint.X - lastPoint.X) > 1 || Math.Abs(firstPoint.Y - lastPoint.Y) > 1)
            {
                points.Add(firstPoint);
            }
        }

        if (points.Count < 3)
        {
            return;
        }

        // Get colors
        var (fgColor, bgColor) = GetISColorFromState(state);
        var brush = Brushes.Solid(fgColor);
        var pen = Pens.Solid(fgColor, GetPenWidth(size));

        // Create and fill the polygon
        var polygon = new Polygon(points.ToArray());

        image.Mutate(ctx =>
        {
            ctx.Fill(brush, polygon);
            ctx.Draw(pen, polygon);
        });

        // Update pen position
        var (normX, normY) = ConvertScreenToNormalizedF(size, currentX, currentY);
        state.Pen = new Vector3(normX, normY, 0);
    }

    // Uses centralized NaplpsUtils.ConvertScreenToNormalizedF
}
