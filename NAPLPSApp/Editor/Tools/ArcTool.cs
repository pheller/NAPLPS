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
            Shape = PreviewShape.Polygon
        };

        foreach (var p in _clickPoints)
        {
            preview.Points.Add(p);
        }

        // Add current mouse position as preview
        preview.Points.Add((CurrentX, CurrentY));

        return preview;
    }
}
