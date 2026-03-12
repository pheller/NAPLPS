// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSApp.Editor;

/// <summary>
/// Abstract base class for visual editor tools.
/// Each tool handles pointer events and commits NAPLPS commands.
/// </summary>
public abstract class EditorToolBase
{
    public abstract string Name { get; }

    /// <summary>Called when the pointer is pressed on the canvas.</summary>
    /// <param name="normX">NAPLPS normalized X [0,1]</param>
    /// <param name="normY">NAPLPS normalized Y [0,0.75]</param>
    /// <param name="isRightButton">True if right mouse button was pressed</param>
    public abstract void OnPointerPressed(float normX, float normY, bool isRightButton);

    /// <summary>Called when the pointer moves on the canvas.</summary>
    public abstract void OnPointerMoved(float normX, float normY);

    /// <summary>Called when the pointer is released on the canvas.</summary>
    /// <returns>List of (opcode, operands) tuples to commit as commands, or empty if nothing to commit.</returns>
    public abstract List<(byte opcode, NaplpsOperands operands)> OnPointerReleased(float normX, float normY);

    /// <summary>Returns a preview shape for rubber-band display, or null if nothing to preview.</summary>
    public virtual ToolPreview? GetPreview() => null;

    /// <summary>Whether the tool is currently in a drag operation.</summary>
    public bool IsDragging { get; protected set; }

    /// <summary>Start position of the current drag in NAPLPS coords.</summary>
    public float StartX { get; protected set; }
    public float StartY { get; protected set; }

    /// <summary>Current position during drag.</summary>
    public float CurrentX { get; protected set; }
    public float CurrentY { get; protected set; }
}
