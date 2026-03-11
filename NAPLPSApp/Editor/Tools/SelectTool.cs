// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSApp.Editor.Tools;

/// <summary>
/// Selection tool. Clicking on the canvas hit-tests commands and selects them.
/// Does not create any commands on release.
/// </summary>
public class SelectTool : EditorToolBase
{
    public override string Name => "Select";

    /// <summary>The NaplpsFormat to hit-test against. Set by the ViewModel when this tool activates.</summary>
    public NaplpsFormat? Format { get; set; }

    /// <summary>Index of the currently selected command, or -1 for no selection.</summary>
    public int SelectedIndex { get; set; } = -1;

    public override void OnPointerPressed(float normX, float normY, bool isRightButton)
    {
        if (Format == null)
        {
            SelectedIndex = -1;
            return;
        }

        SelectedIndex = CommandHitTester.HitTest(Format, normX, normY);
    }

    public override void OnPointerMoved(float normX, float normY) { }

    public override List<(byte opcode, NaplpsOperands operands)> OnPointerReleased(float normX, float normY)
    {
        return [];
    }

    public override ToolPreview? GetPreview()
    {
        if (SelectedIndex < 0 || Format == null) return null;

        var bbox = CommandHitTester.GetBoundingBox(Format, SelectedIndex);
        if (bbox == null) return null;

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
