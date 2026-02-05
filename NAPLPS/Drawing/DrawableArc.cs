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

        var startPoint = ConvertNormalizedToPoint(size, _command.StartPoint.X, _command.StartPoint.Y);
        var midPoint = ConvertNormalizedToPoint(size, _command.IntermediatePointDisplacement.X, _command.IntermediatePointDisplacement.Y);
        var endPoint = ConvertNormalizedToPoint(size, _command.EndPointDisplacement.X, _command.EndPointDisplacement.Y);

        if (startPoint == endPoint)
        {
            // circle
            var circleDiameter = CalculateDistance(midPoint, startPoint);
            if (circleDiameter == 0)
            {
                // Per NAPLPS spec: if diameter is zero, use smallest value possible
                // within the current domain (typically 1 pixel)
                circleDiameter = Math.Max(1, GetPenWidth(size));
            }

            float circleRadius = (float)(circleDiameter * 0.5f);
            float centerX = (startPoint.X + midPoint.X) * 0.5f;
            float centerY = (startPoint.Y + midPoint.Y) * 0.5f;

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
            // Calculate circle properties that approximately fits the points
            var center = CalculateCircleCenter(startPoint, midPoint, endPoint);

            var circleDiameter = (float)CalculateDistance(midPoint, startPoint);
            var circleRadius = circleDiameter / 2;

            // Create size based on radius
            SizeF circleSize = new SizeF(2 * circleRadius, 2 * circleRadius);

            // Calculate angles
            float startAngle = AngleFromCenter(center, startPoint);
            float endAngle = AngleFromCenter(center, endPoint);

            // Sweep direction and large arc
            bool sweepDirection = endAngle < startAngle;
            float angleDifference = endAngle - startAngle;
            if (angleDifference < 0) angleDifference += 360;
            bool isLargeArc = angleDifference > 180;

            // Assuming rotationAngle is 0 for simplicity, more complex for ellipse
            float rotationAngle = 0;

            var spline = new ArcLineSegment(new(startPoint.X, startPoint.Y), new(endPoint.X, endPoint.Y), circleSize, rotationAngle, isLargeArc, sweepDirection);

            image.Mutate(x => x.DrawPolygon(pen, spline.Flatten().ToArray()));
        }

        /* old attempt */
        //var startPoint = NaplpsUtils.ConvertNormalizedToPoint(size, _command.StartPoint.X, _command.StartPoint.Y);
        //var midPoint = NaplpsUtils.ConvertNormalizedToPoint(size, _command.StartPoint.X + _command.IntermediatePointDisplacement.X, _command.StartPoint.Y + _command.IntermediatePointDisplacement.Y);
        //var endPoint = NaplpsUtils.ConvertNormalizedToPoint(size, _command.EndPointDisplacement.X, _command.EndPointDisplacement.Y);

        //var fgcolor = state.ColorMode == 0 ? state.Foreground.ToColor() : state.ColorMap[state.ColorMapForeground].ToColor();
        //var bgcolor = state.ColorMode == 0 ? state.Background.ToColor() : state.ColorMap[state.ColorMapBackground].ToColor();

        //if (startPoint == endPoint)
        //{
        //    // circle
        //    var circleDiameter = NaplpsUtils.CalculateDistance(midPoint, startPoint);
        //    var circleRadius = circleDiameter / 2;

        //    var circle = new EllipsePolygon(new PointF(startPoint.X, startPoint.Y), (float)circleRadius);

        //    var pen = Pens.Solid(Color.FromRgba(fgcolor.R, fgcolor.G, fgcolor.B, fgcolor.A), 1f);
        //    var brush = Brushes.Solid(Color.FromRgba(fgcolor.R, fgcolor.G, fgcolor.B, fgcolor.A));

        //    image.Mutate(x => x.Fill(brush, circle).Draw(pen, circle));
        //}
        //else
        //{
        //    // Calculate circle properties that approximately fits the points
        //    System.Drawing.Point center = CalculateCircleCenter(startPoint, midPoint, endPoint);

        //    var circleDiameter = (float)NaplpsUtils.CalculateDistance(midPoint, startPoint);
        //    var circleRadius = circleDiameter / 2;

        //    // Create size based on radius
        //    SizeF circleSize = new SizeF(2 * circleRadius, 2 * circleRadius);

        //    // Calculate angles
        //    float startAngle = AngleFromCenter(new PointF(center.X, center.Y), new PointF(startPoint.X, startPoint.Y));
        //    float endAngle = AngleFromCenter(new PointF(center.X, center.Y), new PointF(endPoint.X, endPoint.Y));

        //    // Sweep direction and large arc
        //    bool sweepDirection = (endAngle > startAngle);
        //    float angleDifference = endAngle - startAngle;
        //    if (angleDifference < 0) angleDifference += 360;
        //    bool isLargeArc = angleDifference > 180;

        //    // Assuming rotationAngle is 0 for simplicity, more complex for ellipse
        //    float rotationAngle = 0;

        //    var spline = new ArcLineSegment(new PointF(startPoint.X, startPoint.Y), new PointF(endPoint.X, endPoint.Y), circleSize, rotationAngle, isLargeArc, sweepDirection);

        //    var pen = Pens.Solid(Color.FromRgba(fgcolor.R, fgcolor.G, fgcolor.B, fgcolor.A), 1f);
        //    var brush = Brushes.Solid(Color.FromRgba(fgcolor.R, fgcolor.G, fgcolor.B, fgcolor.A));

        //    image.Mutate(x => x.DrawPolygon(pen, spline.Flatten().ToArray()));
        //}

        // Debugger.Break();

        //image.Mutate(x => x.Draw(pen, lines));
    }

    private static float AngleFromCenter(PointF center, Point point)
    {
        return (float)(Math.Atan2(point.Y - center.Y, point.X - center.X) * (180 / Math.PI));
    }

    private static PointF CalculateCircleCenter(Point A, Point B, Point C)
    {
        // Similar calculation as previously, assuming non-collinear points
        int D = 2 * (A.X * (B.Y - C.Y) + B.X * (C.Y - A.Y) + C.X * (A.Y - B.Y));

        if (D == 0)
        {
            return PointF.Empty;
        }

        PointF center = new(
            ((A.X * A.X + A.Y * A.Y) * (B.Y - C.Y) + (B.X * B.X + B.Y * B.Y) * (C.Y - A.Y) + (C.X * C.X + C.Y * C.Y) * (A.Y - B.Y)) / D,
            ((A.X * A.X + A.Y * A.Y) * (C.X - B.X) + (B.X * B.X + B.Y * B.Y) * (A.X - C.X) + (C.X * C.X + C.Y * C.Y) * (B.X - A.X)) / D
        );

        return center;
    }
}
