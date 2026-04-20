// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSApp.Editor.Tools;

/// <summary>
/// 3-click arc tool: start, mid, end points.
/// Creates PointSetAbsolute + ArcOutlined/ArcFilled commands.
/// </summary>
public class ArcTool : EditorToolBase
{
    public override string Name => "Arc";

    public bool IsFilled { get; set; } = false;

    public override bool EmitsFilledGeometry => IsFilled;

    private readonly List<(float X, float Y)> _clickPoints = [];

    public override void OnPointerPressed(float normX, float normY, bool isRightButton)
    {
        if (isRightButton)
        {
            _clickPoints.Clear();
            IsDragging = false;
            return;
        }

        _clickPoints.Add((normX, normY));
        CurrentX = normX;
        CurrentY = normY;
        IsDragging = _clickPoints.Count < 3;
    }

    public override void OnPointerMoved(float normX, float normY)
    {
        CurrentX = normX;
        CurrentY = normY;
    }

    public override List<(byte opcode, NaplpsOperands operands)> OnPointerReleased(float normX, float normY)
    {
        if (_clickPoints.Count < 3)
        {
            return [];
        }

        // We have 3 clicks: start, mid, end
        var start = _clickPoints[0];
        var mid = _clickPoints[1];
        var end = _clickPoints[2];

        var commands = new List<(byte opcode, NaplpsOperands operands)>();

        // Move pen to start
        commands.Add(NaplpsCommandBuilder.BuildPointSetAbsolute(start.X, start.Y));

        // Arc operands are relative: mid-start, end-start
        float midRelX = mid.X - start.X;
        float midRelY = mid.Y - start.Y;
        float endRelX = end.X - start.X;
        float endRelY = end.Y - start.Y;

        if (IsFilled)
        {
            commands.Add(NaplpsCommandBuilder.BuildArcFilled(midRelX, midRelY, endRelX, endRelY));
        }
        else
        {
            commands.Add(NaplpsCommandBuilder.BuildArcOutlined(midRelX, midRelY, endRelX, endRelY));
        }

        _clickPoints.Clear();
        IsDragging = false;
        return commands;
    }

    public override ToolPreview? GetPreview()
    {
        if (_clickPoints.Count == 0)
        {
            return null;
        }

        var preview = new ToolPreview
        {
            Shape = PreviewShape.Polygon,
            IsFilled = IsFilled
        };

        // Show the clicks the user has already planted so they know where their anchors are.
        foreach (var p in _clickPoints)
        {
            preview.Handles.Add(p);
        }

        if (_clickPoints.Count == 1)
        {
            // First segment — just a straight line from the start to the cursor. No arc yet.
            preview.Points.Add(_clickPoints[0]);
            preview.Points.Add((CurrentX, CurrentY));
            return preview;
        }

        // 2 clicks: live arc through start, mid-click, and cursor-as-end.
        // 3 clicks: frozen arc through all three (brief window between press-3 and release-3).
        var start = _clickPoints[0];
        var mid   = _clickPoints[1];
        var end   = _clickPoints.Count >= 3 ? _clickPoints[2] : (CurrentX, CurrentY);

        foreach (var p in SampleArcCurve(start, mid, end))
        {
            preview.Points.Add(p);
        }

        return preview;
    }

    /// <summary>Sample N points along the circular arc that passes through a, b, c
    /// (in that order). Falls back to the straight polyline when the three points are
    /// near-collinear so the preview stays sensible at degenerate configurations.</summary>
    private static List<(float X, float Y)> SampleArcCurve((float X, float Y) a, (float X, float Y) b, (float X, float Y) c)
    {
        // Circumcenter of triangle ABC. |D| proportional to twice the signed area — small
        // means near-collinear, so no well-defined circle.
        float D = 2f * (a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y));

        if (MathF.Abs(D) < 1e-6f)
        {
            return [a, b, c];
        }

        float aSq = a.X * a.X + a.Y * a.Y;
        float bSq = b.X * b.X + b.Y * b.Y;
        float cSq = c.X * c.X + c.Y * c.Y;

        float cx = (aSq * (b.Y - c.Y) + bSq * (c.Y - a.Y) + cSq * (a.Y - b.Y)) / D;
        float cy = (aSq * (c.X - b.X) + bSq * (a.X - c.X) + cSq * (b.X - a.X)) / D;

        float radius = MathF.Sqrt((a.X - cx) * (a.X - cx) + (a.Y - cy) * (a.Y - cy));

        float angleA = MathF.Atan2(a.Y - cy, a.X - cx);
        float angleB = MathF.Atan2(b.Y - cy, b.X - cx);
        float angleC = MathF.Atan2(c.Y - cy, c.X - cx);

        // Direction choice: sweep from A to C the way that actually passes through B.
        float ccwFull = NormalizeAngle(angleC - angleA);
        float ccwMid  = NormalizeAngle(angleB - angleA);
        bool sweepCCW = ccwMid < ccwFull;

        float sweep = sweepCCW ? ccwFull : ccwFull - 2f * MathF.PI;

        const int segments = 48;
        var pts = new List<(float X, float Y)>(segments + 1);

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = angleA + t * sweep;
            pts.Add((cx + radius * MathF.Cos(angle), cy + radius * MathF.Sin(angle)));
        }

        return pts;
    }

    private static float NormalizeAngle(float a)
    {
        while (a < 0f)            { a += 2f * MathF.PI; }
        while (a >= 2f * MathF.PI) { a -= 2f * MathF.PI; }
        return a;
    }
}
