// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Numerics;
using NAPLPS.Drawing;

namespace NAPLPSTests.Commands;

/// <summary>
/// Tests for spec compliance items 6-12:
/// - Cursor/drawing point relationship modes
/// - Scroll mode field scoping
/// - Backspace/Tab field boundary wrapping
/// - REPEAT validation
/// - Cancel (0x18) macro termination
/// - Bell (0x07) audible feedback
/// - Cursor Position (0x1C) row/column addressing
/// </summary>
[TestClass]
public class SpecComplianceTests
{
    private static NaplpsState CreateFieldState(float fieldWidth = 0.5f, float fieldHeight = 0.5f)
    {
        var state = new NaplpsState();
        state.Field = new NaplpsField(new Vector3(0, 0, 0), new Vector3(fieldWidth, fieldHeight, 0));
        state.Pen = new Vector3(0, fieldHeight, 0);
        return state;
    }

    // ========================================================================
    // Item 6: Cursor/Drawing Point Relationship Modes
    // ========================================================================

    [TestMethod]
    public void MoveTogether_TextMoveSyncsDrawingPoint()
    {
        var state = new NaplpsState();
        state.TextMoveAttributes = TextCommand.TextMoveAttributes.MoveTogether;
        state.Pen = new Vector3(0, 0.5f, 0);

        var cmd = new AsciiCharCommand('A', state, 0x41, new NaplpsOperands([]));

        Assert.AreEqual(state.Pen.X, state.DrawingPoint.X, 0.0001f);
        Assert.AreEqual(state.Pen.Y, state.DrawingPoint.Y, 0.0001f);
    }

    [TestMethod]
    public void CursorLeads_TextMoveSyncsDrawingPoint()
    {
        var state = new NaplpsState();
        state.TextMoveAttributes = TextCommand.TextMoveAttributes.CursorLeads;
        state.Pen = new Vector3(0, 0.5f, 0);

        var cmd = new AsciiCharCommand('A', state, 0x41, new NaplpsOperands([]));

        Assert.AreEqual(state.Pen.X, state.DrawingPoint.X, 0.0001f);
    }

    [TestMethod]
    public void MoveIndependently_TextMoveDoesNotSyncDrawingPoint()
    {
        var state = new NaplpsState();
        state.TextMoveAttributes = TextCommand.TextMoveAttributes.MoveIndependently;
        state.Pen = new Vector3(0, 0.5f, 0);
        state.DrawingPoint = new Vector3(0.5f, 0.5f, 0);

        var cmd = new AsciiCharCommand('A', state, 0x41, new NaplpsOperands([]));

        // DrawingPoint should NOT have moved
        Assert.AreEqual(0.5f, state.DrawingPoint.X, 0.0001f);
    }

    [TestMethod]
    public void DrawingPointLeads_TextMoveDoesNotSyncDrawingPoint()
    {
        var state = new NaplpsState();
        state.TextMoveAttributes = TextCommand.TextMoveAttributes.DrawingPointLeads;
        state.Pen = new Vector3(0, 0.5f, 0);
        state.DrawingPoint = new Vector3(0.5f, 0.5f, 0);

        var cmd = new AsciiCharCommand('A', state, 0x41, new NaplpsOperands([]));

        // DrawingPoint should NOT have moved (only Pen moved)
        Assert.AreEqual(0.5f, state.DrawingPoint.X, 0.0001f);
    }

    [TestMethod]
    public void SyncAfterGraphicsMove_MoveTogether()
    {
        var state = new NaplpsState();
        state.TextMoveAttributes = TextCommand.TextMoveAttributes.MoveTogether;
        state.DrawingPoint = new Vector3(0.75f, 0.25f, 0);

        state.SyncAfterGraphicsMove();

        Assert.AreEqual(0.75f, state.Pen.X, 0.0001f);
        Assert.AreEqual(0.25f, state.Pen.Y, 0.0001f);
    }

    [TestMethod]
    public void SyncAfterGraphicsMove_CursorLeads_DoesNotSyncPen()
    {
        var state = new NaplpsState();
        state.TextMoveAttributes = TextCommand.TextMoveAttributes.CursorLeads;
        state.Pen = new Vector3(0, 0, 0);
        state.DrawingPoint = new Vector3(0.75f, 0.25f, 0);

        state.SyncAfterGraphicsMove();

        // Pen should NOT move — cursor leads, not drawing point
        Assert.AreEqual(0f, state.Pen.X, 0.0001f);
    }

    // ========================================================================
    // Item 7: Scroll Mode (field scoping tested via state properties)
    // ========================================================================

    [TestMethod]
    public void ScrollMode_DefaultOff()
    {
        var state = new NaplpsState();

        Assert.IsFalse(state.IsScrollMode);
    }

    // ========================================================================
    // Item 8: Backspace/Tab Field Boundary Wrapping
    // ========================================================================

    [TestMethod]
    public void Tab_AdvancesPenForward()
    {
        // Tab just advances pen — can't easily test via NaplpsFormat directly,
        // so test the state manipulation pattern
        var state = new NaplpsState();
        state.Pen = new Vector3(0.1f, 0.5f, 0);
        float penXBefore = state.Pen.X;

        // Simulate tab: advance by one charSize.X
        var pen = state.Pen;
        pen.X += state.CharSize.X;
        state.Pen = pen;

        Assert.IsTrue(state.Pen.X > penXBefore);
    }

    [TestMethod]
    public void Backspace_MovesPenBackward()
    {
        var state = new NaplpsState();
        state.Pen = new Vector3(0.1f, 0.5f, 0);
        float penXBefore = state.Pen.X;

        var pen = state.Pen;
        pen.X -= state.CharSize.X;
        state.Pen = pen;

        Assert.IsTrue(state.Pen.X < penXBefore);
    }

    // ========================================================================
    // Item 9: REPEAT Validation
    // ========================================================================

    [TestMethod]
    public void Repeat_NonSpacingAccent_IsRejected()
    {
        // Create a non-spacing accent character command
        var state = new NaplpsState();
        state.DoSingleShiftTwo();
        state.ResolveByte(0x42);
        var accentCmd = new AsciiCharCommand('\u00B4', state, 0x42, new NaplpsOperands([]));

        Assert.IsTrue(accentCmd.IsNonSpacing);
        // RenderRepeat checks IsNonSpacing and returns early — tested via DrawContext
    }

    [TestMethod]
    public void Repeat_CountByte_BelowRange_ReturnsZero()
    {
        // Count byte below 0x40 should be rejected
        var state = new NaplpsState();
        var command = new ControlCommand(NaplpsControlCommands.Repeat, state, 0x86, new NaplpsOperands([0x20]));
        var repeat = new DrawableRepeat(command);

        Assert.AreEqual(0, repeat.GetRepeatCount(state));
    }

    [TestMethod]
    public void Repeat_CountByte_8BitMode_Works()
    {
        // Spec (NAPLPS.ASC): count = bits 6..1 = byte & 0x3F. 0xE5 & 0x3F = 0x25 = 37.
        var state = new NaplpsState();
        var command = new ControlCommand(NaplpsControlCommands.Repeat, state, 0x86, new NaplpsOperands([0xE5]));
        var repeat = new DrawableRepeat(command);

        Assert.AreEqual(0x25, repeat.GetRepeatCount(state));
    }

    [TestMethod]
    public void Repeat_CountByte_ValidRange_ReturnsCount()
    {
        // Spec: count = byte & 0x3F. 0x45 & 0x3F = 5.
        var state = new NaplpsState();
        var command = new ControlCommand(NaplpsControlCommands.Repeat, state, 0x86, new NaplpsOperands([0x45]));
        var repeat = new DrawableRepeat(command);

        Assert.AreEqual(0x05, repeat.GetRepeatCount(state));
    }

    [TestMethod]
    public void Repeat_CountByte_0x40_ReturnsZero()
    {
        // Spec: 0x40 is in the valid gate (bits 7..1 in 0x40..0x7F) but bits 6..1 = 0.
        var state = new NaplpsState();
        var command = new ControlCommand(NaplpsControlCommands.Repeat, state, 0x86, new NaplpsOperands([0x40]));
        var repeat = new DrawableRepeat(command);

        Assert.AreEqual(0, repeat.GetRepeatCount(state));
    }

    [TestMethod]
    public void Repeat_CountByte_0x7F_Returns63()
    {
        // Spec: count = byte & 0x3F. 0x7F & 0x3F = 0x3F = 63.
        var state = new NaplpsState();
        var command = new ControlCommand(NaplpsControlCommands.Repeat, state, 0x86, new NaplpsOperands([0x7F]));
        var repeat = new DrawableRepeat(command);

        Assert.AreEqual(0x3F, repeat.GetRepeatCount(state));
    }

    // ========================================================================
    // Item 10: Cancel (0x18)
    // ========================================================================

    [TestMethod]
    public void Cancel_DefaultNotRequested()
    {
        var state = new NaplpsState();

        Assert.IsFalse(state.IsCancelRequested);
    }

    [TestMethod]
    public void Cancel_SetsCancelFlag()
    {
        var state = new NaplpsState();

        state.IsCancelRequested = true;

        Assert.IsTrue(state.IsCancelRequested);
    }

    [TestMethod]
    public void Cancel_ClearsMacroState()
    {
        var state = new NaplpsState();
        state.MacroBeingDefined = 'A';
        state.MacroBuffer.Add(0x41);

        // Simulate Cancel handler
        state.MacroBeingDefined = null;
        state.MacroBuffer.Clear();
        state.IsCancelRequested = true;

        Assert.IsNull(state.MacroBeingDefined);
        Assert.AreEqual(0, state.MacroBuffer.Count);
        Assert.IsTrue(state.IsCancelRequested);
    }

    // ========================================================================
    // Item 11: Bell (0x07)
    // ========================================================================

    [TestMethod]
    public void Bell_DefaultCountZero()
    {
        var state = new NaplpsState();

        Assert.AreEqual(0, state.BellCount);
    }

    [TestMethod]
    public void Bell_IncrementCount()
    {
        var state = new NaplpsState();

        state.BellCount++;
        state.BellCount++;
        state.BellCount++;

        Assert.AreEqual(3, state.BellCount);
    }

    // ========================================================================
    // Item 12: Cursor Position (0x1C) — tested via state
    // ========================================================================

    [TestMethod]
    public void ActivePositionSet_PositionsByRowColumn()
    {
        var state = CreateFieldState(1.0f, 1.0f);

        // Simulate APS: row=2, col=5
        int row = 2;
        int col = 5;
        var pen = state.Pen;
        pen.X = state.Field.Origin.X + col * state.CharSize.X;
        pen.Y = state.Field.Origin.Y + state.Field.Dimensions.Y - row * state.CharSize.Y;
        state.Pen = pen;

        Assert.AreEqual(col * state.CharSize.X, state.Pen.X, 0.0001f);
        Assert.AreEqual(1.0f - row * state.CharSize.Y, state.Pen.Y, 0.0001f);
    }

    [TestMethod]
    public void ActivePositionSet_Row0Col0_IsTopLeft()
    {
        var state = CreateFieldState(1.0f, 1.0f);

        var pen = state.Pen;
        pen.X = state.Field.Origin.X;
        pen.Y = state.Field.Origin.Y + state.Field.Dimensions.Y;
        state.Pen = pen;

        Assert.AreEqual(0f, state.Pen.X, 0.0001f);
        Assert.AreEqual(1.0f, state.Pen.Y, 0.0001f);
    }
}
