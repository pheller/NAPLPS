// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Numerics;

namespace NAPLPSApp.Editor.Tools;

/// <summary>
/// Multi-click polygon tool. Click adds vertices, double-click finalizes.
/// Creates PointSetAbsolute + PolygonFilled/Outlined commands.
/// </summary>
public class PolygonTool : EditorToolBase
{
    public override string Name => "Polygon";

    public bool IsFilled { get; set; } = true;

    private readonly List<(float X, float Y)> _vertices = [];

    /// <summary>Set by code-behind from PointerPressedEventArgs.ClickCount.</summary>
    public int ClickCount { get; set; } = 1;

    public override void OnPointerPressed(float normX, float normY, bool isRightButton)
    {
        if (isRightButton)
        {
            // Right click cancels polygon
            _vertices.Clear();
            IsDragging = false;
            return;
        }

        if (ClickCount >= 2 && _vertices.Count >= 2)
        {
            // Double-click finalizes
            IsDragging = false;
            return;
        }

        _vertices.Add((normX, normY));
        CurrentX = normX;
        CurrentY = normY;
        IsDragging = true;
    }

    public override void OnPointerMoved(float normX, float normY)
    {
        CurrentX = normX;
        CurrentY = normY;
    }

    public override void Reset()
    {
        _vertices.Clear();
        ClickCount = 1;
        base.Reset();
    }

    public override List<(byte opcode, NaplpsOperands operands)> OnPointerReleased(float normX, float normY)
    {
        if (ClickCount < 2 || _vertices.Count < 3)
        {
            // Not finalizing yet
            return [];
        }

        return Finalize();
    }

    private List<(byte opcode, NaplpsOperands operands)> Finalize()
    {
        if (_vertices.Count < 3)
        {
            _vertices.Clear();
            return [];
        }

        var commands = new List<(byte opcode, NaplpsOperands operands)>();

        // Move pen to first vertex
        commands.Add(NaplpsCommandBuilder.BuildPointSetAbsolute(_vertices[0].X, _vertices[0].Y));

        // Build polygon with relative vertices from first point
        var relativeVerts = new Vector3[_vertices.Count - 1];

        for (int i = 1; i < _vertices.Count; i++)
        {
            relativeVerts[i - 1] = new Vector3(
                _vertices[i].X - _vertices[i - 1].X,
                _vertices[i].Y - _vertices[i - 1].Y,
                0);
        }

        if (IsFilled)
        {
            commands.Add(NaplpsCommandBuilder.BuildPolygonFilled(relativeVerts));
        }
        else
        {
            commands.Add(NaplpsCommandBuilder.BuildPolygonOutlined(relativeVerts));
        }

        _vertices.Clear();
        IsDragging = false;
        return commands;
    }

    public override ToolPreview? GetPreview()
    {
        if (_vertices.Count == 0)
        {
            return null;
        }

        var preview = new ToolPreview
        {
            Shape = PreviewShape.Polygon,
            IsFilled = IsFilled
        };

        foreach (var v in _vertices)
        {
            preview.Points.Add(v);
        }

        // Add current mouse position as a live preview vertex
        preview.Points.Add((CurrentX, CurrentY));

        return preview;
    }
}
