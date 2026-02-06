// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using PointF = SixLabors.ImageSharp.PointF;
using SizeF = SixLabors.ImageSharp.SizeF;

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
        bool isCircle = Math.Abs(_command.StartPoint.X - _command.EndPointDisplacement.X) < 0.0001f &&
                        Math.Abs(_command.StartPoint.Y - _command.EndPointDisplacement.Y) < 0.0001f;

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

            SizeF arcSize = new SizeF(radius * 2, radius * 2);

            // Calculate angles from center
            float startAngle = (float)(Math.Atan2(startY - center.Y, startX - center.X) * (180 / Math.PI));
            float midAngle = (float)(Math.Atan2(midY - center.Y, midX - center.X) * (180 / Math.PI));
            float endAngle = (float)(Math.Atan2(endY - center.Y, endX - center.X) * (180 / Math.PI));

            // Determine sweep direction based on whether mid point is in the arc
            // The mid point should be between start and end on the arc
            bool sweepClockwise = IsClockwise(startAngle, midAngle, endAngle);

            // Determine if it's a large arc (> 180 degrees)
            float angleDiff = endAngle - startAngle;
            if (angleDiff < 0) angleDiff += 360;
            if (angleDiff > 360) angleDiff -= 360;

            bool isLargeArc = angleDiff > 180;

            // Adjust based on sweep direction
            if (!sweepClockwise)
            {
                isLargeArc = !isLargeArc;
            }

            try
            {
                var arcSegment = new ArcLineSegment(
                    new PointF(startX, startY),
                    new PointF(endX, endY),
                    arcSize,
                    0, // rotation angle
                    isLargeArc,
                    sweepClockwise
                );

                var arcPoints = arcSegment.Flatten().ToArray();

                if (arcPoints.Length >= 2)
                {
                    image.Mutate(x =>
                    {
                        if (_command.ShouldFill)
                        {
                            // For filled arc, create a polygon with the arc and chord
                            var fillPoints = new List<PointF>(arcPoints);
                            fillPoints.Add(new PointF(startX, startY)); // Close the shape
                            x.FillPolygon(brush, fillPoints.ToArray());
                        }

                        // Draw the arc outline (not the chord)
                        x.DrawLine(pen, arcPoints);
                    });
                }
            }
            catch
            {
                // Fallback: draw line if arc construction fails
                image.Mutate(x => x.DrawLine(pen, new PointF(startX, startY), new PointF(endX, endY)));
            }
        }
    }

    private static (float, float) ConvertNormalizedToScreenF(Size size, float x, float y)
    {
        float screenX = x * size.Width;
        float screenY = size.Height - (y / 0.75f * size.Height);
        return (screenX, screenY);
    }

    private static float CalculateDistanceF(float x1, float y1, float x2, float y2)
    {
        float dx = x2 - x1;
        float dy = y2 - y1;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    private static bool IsClockwise(float startAngle, float midAngle, float endAngle)
    {
        // Normalize angles to 0-360
        startAngle = NormalizeAngle(startAngle);
        midAngle = NormalizeAngle(midAngle);
        endAngle = NormalizeAngle(endAngle);

        // Check if going from start to mid to end is clockwise
        float startToMid = NormalizeAngle(midAngle - startAngle);
        float startToEnd = NormalizeAngle(endAngle - startAngle);

        // If mid is between start and end going clockwise, then clockwise sweep
        return startToMid < startToEnd;
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle < 0) angle += 360;
        while (angle >= 360) angle -= 360;
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
