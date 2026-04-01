// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using PointF = SixLabors.ImageSharp.PointF;

namespace NAPLPS.Drawing;

public class DrawableLine : Drawable, IDrawable
{
    private readonly LineCommand _command;

    public DrawableLine(LineCommand command) : base(command)
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

        var palette = (Drawable.UseLivePalette && Drawable.LivePalette != null) ? Drawable.LivePalette : state.ColorMap;
        var color = state.ColorMode == 0 ? state.Foreground.ToColor() : palette[state.ColorMapForeground].ToColor();
        var isColor = color.ToISColor();

        // NAPLPS spec: lines are drawn by sweeping the rectangular logical pel along the path.
        // The pel is NOT centered — its origin corner is determined by the sign of pel dimensions.
        var (dxMin, dxMax, dyMin, dyMax) = GetPelOffsets(size);

        image.Mutate(ctx =>
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                var p1 = points[i];
                var p2 = points[i + 1];

                var hull = ConvexHullOfSweptPel(p1, p2, dxMin, dxMax, dyMin, dyMax);
                ctx.FillPolygon(isColor, hull);
            }
        });
    }

    /// <summary>
    /// Computes the convex hull of the logical pel rectangle positioned at two points.
    /// Default: pel extends RIGHT by pelW and UP by pelH (positive NAPLPS dimensions).
    /// </summary>
    internal static PointF[] ConvexHullOfSweptPel(PointF p1, PointF p2, float pelW, float pelH)
    {
        return ConvexHullOfSweptPel(p1, p2, 0, pelW, -pelH, 0);
    }

    /// <summary>
    /// Computes the convex hull of the logical pel rectangle positioned at two points,
    /// with sign-aware offsets per ANSI X3.110 logical pel origin rules.
    /// (dxMin, dxMax, dyMin, dyMax) define the pel rectangle offset from the drawing point.
    /// </summary>
    internal static PointF[] ConvexHullOfSweptPel(PointF p1, PointF p2, float dxMin, float dxMax, float dyMin, float dyMax)
    {
        // 4 corners of pel at P1 + 4 corners of pel at P2 = 8 points
        var allPoints = new PointF[8];
        allPoints[0] = new PointF(p1.X + dxMin, p1.Y + dyMin);
        allPoints[1] = new PointF(p1.X + dxMax, p1.Y + dyMin);
        allPoints[2] = new PointF(p1.X + dxMax, p1.Y + dyMax);
        allPoints[3] = new PointF(p1.X + dxMin, p1.Y + dyMax);
        allPoints[4] = new PointF(p2.X + dxMin, p2.Y + dyMin);
        allPoints[5] = new PointF(p2.X + dxMax, p2.Y + dyMin);
        allPoints[6] = new PointF(p2.X + dxMax, p2.Y + dyMax);
        allPoints[7] = new PointF(p2.X + dxMin, p2.Y + dyMax);

        return ComputeConvexHull(allPoints);
    }

    /// <summary>
    /// Andrew's monotone chain convex hull algorithm. O(n log n).
    /// Returns vertices in counter-clockwise order.
    /// </summary>
    private static PointF[] ComputeConvexHull(PointF[] points)
    {
        int n = points.Length;
        if (n < 3)
        {
            return points;
        }

        // Sort by X, then by Y
        Array.Sort(points, (a, b) => a.X == b.X ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X));

        var hull = new PointF[2 * n];
        int k = 0;

        // Build lower hull
        for (int i = 0; i < n; i++)
        {
            while (k >= 2 && Cross(hull[k - 2], hull[k - 1], points[i]) <= 0)
            {
                k--;
            }

            hull[k++] = points[i];
        }

        // Build upper hull
        int lower = k + 1;

        for (int i = n - 2; i >= 0; i--)
        {
            while (k >= lower && Cross(hull[k - 2], hull[k - 1], points[i]) <= 0)
            {
                k--;
            }

            hull[k++] = points[i];
        }

        // Remove the last point (duplicate of the first)
        var result = new PointF[k - 1];
        Array.Copy(hull, result, k - 1);
        return result;
    }

    private static float Cross(PointF o, PointF a, PointF b)
    {
        return (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
    }
}
