// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSApp.Editor.Tools;

/// <summary>
/// Selection tool. Click hit-tests; Shift/Ctrl modifiers add to the selection.
/// Click-drag rubber-bands a rectangle and selects every command intersecting it.
/// Does not commit any commands on release.
/// </summary>
public class SelectTool : EditorToolBase
{
    public override string Name => "Select";

    /// <summary>The NaplpsFormat to hit-test against. Set by the ViewModel when this tool activates.</summary>
    public NaplpsFormat? Format { get; set; }

    /// <summary>Index of the primary selected command, or -1 for no selection. Backwards-compat with the single-select path.</summary>
    public int SelectedIndex
    {
        get => SelectedIndices.Count > 0 ? SelectedIndices[^1] : -1;
        set
        {
            SelectedIndices.Clear();

            if (value >= 0)
            {
                SelectedIndices.Add(value);
            }
        }
    }

    /// <summary>All currently selected command indices in selection order. Last entry is the primary selection.</summary>
    public List<int> SelectedIndices { get; } = [];

    /// <summary>Whether to add to the existing selection on the next pointer press (Shift / Ctrl).</summary>
    public bool AdditiveModifier { get; set; }

    /// <summary>True if a rubber-band rectangle drag is in progress.</summary>
    private bool _isRubberBanding;

    public override void OnPointerPressed(float normX, float normY, bool isRightButton)
    {
        if (Format == null)
        {
            SelectedIndices.Clear();
            return;
        }

        StartX = normX;
        StartY = normY;
        CurrentX = normX;
        CurrentY = normY;
        _isRubberBanding = false;

        var hit = CommandHitTester.HitTest(Format, normX, normY);

        if (hit < 0)
        {
            // Empty space: start a rubber-band drag.
            _isRubberBanding = true;
            IsDragging = true;

            if (!AdditiveModifier)
            {
                SelectedIndices.Clear();
            }

            return;
        }

        if (AdditiveModifier)
        {
            // Toggle: remove if already selected, otherwise add.
            if (!SelectedIndices.Remove(hit))
            {
                SelectedIndices.Add(hit);
            }
        }
        else
        {
            SelectedIndices.Clear();
            SelectedIndices.Add(hit);
        }
    }

    public override void OnPointerMoved(float normX, float normY)
    {
        CurrentX = normX;
        CurrentY = normY;
    }

    public override List<(byte opcode, NaplpsOperands operands)> OnPointerReleased(float normX, float normY)
    {
        if (_isRubberBanding && Format != null)
        {
            // Only consider it a real rubber-band if the drag distance crosses a small threshold;
            // otherwise treat it as a click-on-empty (which already cleared selection).
            float dx = normX - StartX;
            float dy = normY - StartY;

            if (MathF.Abs(dx) > 0.005f || MathF.Abs(dy) > 0.005f)
            {
                var indices = CommandHitTester.HitTestRect(Format, StartX, StartY, normX, normY);

                foreach (var i in indices)
                {
                    if (!SelectedIndices.Contains(i))
                    {
                        SelectedIndices.Add(i);
                    }
                }
            }
        }

        _isRubberBanding = false;
        IsDragging = false;
        return [];
    }

    /// <summary>Select every command in the format. Bound to Ctrl+A.</summary>
    public void SelectAll()
    {
        if (Format == null)
        {
            return;
        }

        SelectedIndices.Clear();

        for (int i = 0; i < Format.Commands.Count; i++)
        {
            SelectedIndices.Add(i);
        }
    }

    public override ToolPreview? GetPreview()
    {
        // Live rubber-band rectangle while dragging.
        if (_isRubberBanding && IsDragging)
        {
            return new ToolPreview
            {
                Shape = PreviewShape.Rectangle,
                X1 = StartX,
                Y1 = StartY,
                X2 = CurrentX,
                Y2 = CurrentY,
                IsSelection = true,
            };
        }

        if (SelectedIndices.Count == 0 || Format == null)
        {
            return null;
        }

        // Show bbox of the primary (last) selection. The canvas can iterate SelectedIndices
        // separately to outline every selected command.
        var primary = SelectedIndices[^1];
        var bbox = CommandHitTester.GetBoundingBox(Format, primary);

        if (bbox == null)
        {
            return null;
        }

        var (x, y, w, h) = bbox.Value;
        return new ToolPreview
        {
            Shape = PreviewShape.Rectangle,
            X1 = x,
            Y1 = y,
            X2 = x + w,
            Y2 = y + h,
            IsSelection = true
        };
    }
}
