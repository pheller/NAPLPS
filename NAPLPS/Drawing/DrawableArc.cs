// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using PointF = SixLabors.ImageSharp.PointF;

namespace NAPLPS.Drawing;

public class DrawableArc : Drawable, IDrawable
{
    private readonly ArcCommand _command;

    public DrawableArc(ArcCommand command) : base(command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, Size size)
    {
        var (brush, pen) = GetBrushAndPenFromFillableCommand(size);

        // Use floating point for precision - don't convert to integer Point yet
        var (startX, startY) = ConvertNormalizedToScreenF(size, _command.StartPoint.X, _command.StartPoint.Y);
        var (midX, midY) = ConvertNormalizedToScreenF(size, _command.IntermediatePointDisplacement.X, _command.IntermediatePointDisplacement.Y);
        var (endX, endY) = ConvertNormalizedToScreenF(size, _command.EndPointDisplacement.X, _command.EndPointDisplacement.Y);

        // Check if start and end are the same (circle) using original normalized coords
        // to avoid precision loss from screen conversion
        bool isCircle = Math.Abs(_command.StartPoint.X - _command.EndPointDisplacement.X) < 0.0001f && Math.Abs(_command.StartPoint.Y - _command.EndPointDisplacement.Y) < 0.0001f;

        if (isCircle)
        {
            // Circle - the diameter is the distance from start to mid point
            var circleDiameter = CalculateDistanceF(startX, startY, midX, midY);
            if (circleDiameter < 1)
            {
                circleDiameter = Math.Max(1, GetPenWidth(size));
            }

            float circleRadius = circleDiameter * 0.5f;
            float centerX = (startX + midX) * 0.5f;
            float centerY = (startY + midY) * 0.5f;

            var circle = new EllipsePolygon(new PointF(centerX, centerY), circleRadius);

            image.Mutate(x =>
            {
                if (_command.ShouldFill)
                {
                    x.Fill(brush, circle);
                }

                if (!_command.ShouldFill || _command.Texture.ShouldHighlight)
                {
                    x.Draw(pen, circle);
                }
            });
        }
        else
        {
            // Arc - calculate circle center from 3 points
            var center = CalculateCircleCenterF(startX, startY, midX, midY, endX, endY);

            if (center == PointF.Empty)
            {
                // Collinear points - draw a line instead
                image.Mutate(x => x.DrawLine(pen, new PointF(startX, startY), new PointF(endX, endY)));
                return;
            }

            // Calculate radius from center to any point (use start)
            float radius = CalculateDistanceF(center.X, center.Y, startX, startY);

            if (radius < 1)
            {
                radius = 1;
            }

            // Direct arc point generation using Atan2 — avoids fragile ArcLineSegment flags
            float startAngle = MathF.Atan2(startY - center.Y, startX - center.X);
            float midAngle = MathF.Atan2(midY - center.Y, midX - center.X);
            float endAngle = MathF.Atan2(endY - center.Y, endX - center.X);

            // Determine sweep direction: normalize angle diffs to [0, 2π)
            float startToMid = NormRadians(midAngle - startAngle);
            float startToEnd = NormRadians(endAngle - startAngle);

            // If mid point falls within [start, end) going CCW, sweep CCW; otherwise sweep CW
            float sweepAngle = (startToMid <= startToEnd) ? startToEnd : startToEnd - 2f * MathF.PI;

            // Generate arc points
            int steps = Math.Max(32, (int)(MathF.Abs(sweepAngle) * radius));
            var arcPoints = new PointF[steps + 1];

            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                float a = startAngle + sweepAngle * t;
                arcPoints[i] = new PointF(center.X + radius * MathF.Cos(a), center.Y + radius * MathF.Sin(a));
            }

            if (arcPoints.Length >= 2)
            {
                image.Mutate(x =>
                {
                    if (_command.ShouldFill)
                    {
                        var fillPoints = new List<PointF>(arcPoints);
                        fillPoints.Add(new PointF(startX, startY));
                        x.FillPolygon(brush, fillPoints.ToArray());
                    }

                    if (!_command.ShouldFill || _command.Texture.ShouldHighlight)
                    {
                        x.DrawLine(pen, arcPoints);
                    }
                });
            }
        }
    }

    // Uses centralized NaplpsUtils.ConvertNormalizedToScreenF

    private static float CalculateDistanceF(float x1, float y1, float x2, float y2)
    {
        float dx = x2 - x1;
        float dy = y2 - y1;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>Normalize an angle in radians to [0, 2π).</summary>
    private static float NormRadians(float angle)
    {
        const float twoPi = 2f * MathF.PI;
        angle %= twoPi;

        if (angle < 0)
        {
            angle += twoPi;
        }

        return angle;
    }

    private static PointF CalculateCircleCenterF(float ax, float ay, float bx, float by, float cx, float cy)
    {
        float d = 2 * (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by));

        if (Math.Abs(d) < 0.0001f)
        {
            return PointF.Empty; // Collinear points
        }

        float ux = ((ax * ax + ay * ay) * (by - cy) + (bx * bx + by * by) * (cy - ay) + (cx * cx + cy * cy) * (ay - by)) / d;
        float uy = ((ax * ax + ay * ay) * (cx - bx) + (bx * bx + by * by) * (ax - cx) + (cx * cx + cy * cy) * (bx - ax)) / d;

        return new PointF(ux, uy);
    }
}
