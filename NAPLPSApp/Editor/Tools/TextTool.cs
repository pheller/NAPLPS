// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSApp.Editor.Tools;

/// <summary>
/// Click to set insertion point, then type to buffer characters.
/// Committed on Enter or tool switch.
/// </summary>
public class TextTool : EditorToolBase
{
    public override string Name => "Text";

    private readonly List<char> _buffer = [];

    /// <summary>Whether we have an active text insertion point.</summary>
    public bool HasInsertionPoint { get; private set; }

    public float InsertX { get; private set; }
    public float InsertY { get; private set; }

    public override void OnPointerPressed(float normX, float normY, bool isRightButton)
    {
        if (isRightButton) return;

        // Commit any existing text first
        // (handled by the ViewModel which checks HasPendingCommit)

        InsertX = normX;
        InsertY = normY;
        HasInsertionPoint = true;
        _buffer.Clear();
    }

    public override void OnPointerMoved(float normX, float normY) { }

    public override List<(byte opcode, NaplpsOperands operands)> OnPointerReleased(float normX, float normY)
    {
        // Text tool doesn't commit on pointer release — it waits for keyboard input
        return [];
    }

    /// <summary>Add a character to the buffer.</summary>
    public void OnKeyDown(char c)
    {
        if (!HasInsertionPoint) return;

        if (c >= 0x20 && c <= 0x7E)
        {
            _buffer.Add(c);
        }
    }

    /// <summary>Whether there are buffered characters ready to commit.</summary>
    public bool HasPendingCommit => HasInsertionPoint && _buffer.Count > 0;

    /// <summary>
    /// Commits the buffered text as NAPLPS commands.
    /// Called on Enter key or tool switch.
    /// </summary>
    public List<(byte opcode, NaplpsOperands operands)> CommitText()
    {
        if (!HasPendingCommit)
        {
            return [];
        }

        var commands = new List<(byte opcode, NaplpsOperands operands)>();

        // Move pen to insertion point
        commands.Add(NaplpsCommandBuilder.BuildPointSetAbsolute(InsertX, InsertY));

        // Each ASCII character is its own opcode (the character code itself)
        foreach (var c in _buffer)
        {
            commands.Add(((byte)c, new NaplpsOperands()));
        }

        _buffer.Clear();
        HasInsertionPoint = false;
        return commands;
    }

    public override ToolPreview? GetPreview()
    {
        if (!HasInsertionPoint) return null;

        // Show a cursor-like preview at insertion point
        return new ToolPreview
        {
            Shape = PreviewShape.Line,
            X1 = InsertX,
            Y1 = InsertY,
            X2 = InsertX,
            Y2 = InsertY + 0.03f // Small vertical line cursor
        };
    }
}
