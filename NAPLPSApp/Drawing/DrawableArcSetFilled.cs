// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS;
using NAPLPS.Commands;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using PointF = SixLabors.ImageSharp.PointF;
using SizeF = SixLabors.ImageSharp.SizeF;

namespace NAPLPSApp.Drawing;

public class DrawableArcSetFilled : Drawable, IDrawable
{
    private readonly ArcSetFilledCommand _command;

    public DrawableArcSetFilled(ArcSetFilledCommand command) : base(command)
    {
        _command = command;
    }

    public void Draw(Image<Rgba32> image, NaplpsState state, System.Drawing.Size size)
    {
        var (brush, pen) = GetBrushAndPenFromState();

        var startPoint = NaplpsUtils.ConvertNormalizedToPoint(size, _command.StartPoint.X, _command.StartPoint.Y);
        var midPoint = NaplpsUtils.ConvertNormalizedToPoint(size, _command.IntermediatePointDisplacement.X, _command.IntermediatePointDisplacement.Y);
        var endPoint = NaplpsUtils.ConvertNormalizedToPoint(size, _command.EndPointDisplacement.X, _command.EndPointDisplacement.Y);

        if (startPoint == endPoint)
        {
            // circle
            // var circleDiameter = NaplpsUtils.CalculateDistance(midPoint, startPoint);
            var circleDiameter = Math.Max(midPoint.X, midPoint.Y);

            if (circleDiameter == 0)
            {
                // TODO: Fix!
                circleDiameter = 1;
            }

            var circleRadius = circleDiameter / 2f;

            var circle = new EllipsePolygon(new PointF(startPoint.X + circleRadius, startPoint.Y - circleRadius), circleRadius);

            image.Mutate(x => x.Fill(brush, circle).Draw(pen, circle));
        }
        else
        {
            // Calculate circle properties that approximately fits the points
            var center = CalculateCircleCenter(startPoint, midPoint, endPoint);

            var circleDiameter = (float)NaplpsUtils.CalculateDistance(midPoint, startPoint);
            var circleRadius = circleDiameter / 2;

            // Create size based on radius
            SizeF circleSize = new SizeF(2 * circleRadius, 2 * circleRadius);

            // Calculate angles
            float startAngle = AngleFromCenter(center, startPoint);
            float endAngle = AngleFromCenter(center, endPoint);

            // Sweep direction and large arc
            bool sweepDirection = (endAngle < startAngle);
            float angleDifference = endAngle - startAngle;
            if (angleDifference < 0) angleDifference += 360;
            bool isLargeArc = angleDifference > 180;

            // Assuming rotationAngle is 0 for simplicity, more complex for ellipse
            float rotationAngle = 0;

            var spline = new ArcLineSegment(new (startPoint.X, startPoint.Y), new(endPoint.X, endPoint.Y), circleSize, rotationAngle, isLargeArc, sweepDirection);

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

    private static float AngleFromCenter(PointF center, System.Drawing.Point point)
    {
        return (float)(Math.Atan2(point.Y - center.Y, point.X - center.X) * (180 / Math.PI));
    }

    private static PointF CalculateCircleCenter(System.Drawing.Point A, System.Drawing.Point B, System.Drawing.Point C)
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
