// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSApp.Editor;

public enum PreviewShape
{
    None,
    Line,
    Rectangle,
    Polygon
}

/// <summary>
/// Data class for rubber-band preview of the current tool operation.
/// Coordinates are in NAPLPS normalized space.
/// </summary>
public class ToolPreview
{
    public PreviewShape Shape { get; set; } = PreviewShape.None;

    public float X1 { get; set; }
    public float Y1 { get; set; }
    public float X2 { get; set; }
    public float Y2 { get; set; }

    /// <summary>For polygon previews — list of (x,y) vertices.</summary>
    public List<(float X, float Y)> Points { get; set; } = [];

    public bool IsFilled { get; set; }

    /// <summary>Whether this is a selection highlight rather than a tool preview.</summary>
    public bool IsSelection { get; set; }
}
