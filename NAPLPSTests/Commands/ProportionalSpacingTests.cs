// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using NAPLPS.Drawing;

namespace NAPLPSTests.Commands;

[TestClass]
public class ProportionalSpacingTests
{
    [TestMethod]
    public void WidthClass_Space_IsMaxWidth()
    {
        // Space (0x20) should be width class 9 (maximum)
        float ratio = DrawableAsciiChar.GetCharWidthRatio(' ');
        Assert.AreEqual(1.0f, ratio);
    }

    [TestMethod]
    public void WidthClass_Exclamation_IsNarrow()
    {
        // '!' should be width class 0 (narrowest)
        float ratio = DrawableAsciiChar.GetCharWidthRatio('!');
        Assert.AreEqual(0.25f, ratio);
    }

    [TestMethod]
    public void ProportionalDisplacement_SmallText_Row8()
    {
        // Character field width 8/256, width class 0 → displacement = 2/256
        float disp = DrawableAsciiChar.GetProportionalDisplacement(8f / 256f, '!'); // '!' is class 0
        Assert.AreEqual(2f / 256f, disp, 0.0001f);
    }

    [TestMethod]
    public void ProportionalDisplacement_SmallText_Row8_WidthClass9()
    {
        // Character field width 8/256, width class 9 → displacement = 8/256
        float disp = DrawableAsciiChar.GetProportionalDisplacement(8f / 256f, ' '); // space is class 9
        Assert.AreEqual(8f / 256f, disp, 0.0001f);
    }

    [TestMethod]
    public void ProportionalDisplacement_SmallText_Row6()
    {
        // Character field width 6/256, width class 0 → displacement = 2/256
        float disp = DrawableAsciiChar.GetProportionalDisplacement(6f / 256f, '!');
        Assert.AreEqual(2f / 256f, disp, 0.0001f);
    }

    [TestMethod]
    public void ProportionalDisplacement_SmallText_Row10()
    {
        // Character field width 10/256, width class 5 → displacement = 8/256
        float disp = DrawableAsciiChar.GetProportionalDisplacement(10f / 256f, 'A'); // 'A' is class 5
        Assert.AreEqual(8f / 256f, disp, 0.0001f);
    }

    [TestMethod]
    public void ProportionalDisplacement_LargeText_WidthClass0()
    {
        // Large text (n=12): PP3 clamps to row 11 (no Phase 2).
        // Row 11, class 0: displacement = 3/256
        float disp = DrawableAsciiChar.GetProportionalDisplacement(12f / 256f, '!');
        Assert.AreEqual(3f / 256f, disp, 0.002f);
    }

    [TestMethod]
    public void ProportionalDisplacement_DefaultCharSize()
    {
        // Default char size: 1/40 = 0.025. Multiply by 256 = 6.4, truncate to 6.
        // This should use row 6 of the displacement table.
        float defaultWidth = 1.0f / 40.0f;
        float dispSpace = DrawableAsciiChar.GetProportionalDisplacement(defaultWidth, ' ');

        // Row 6, class 9 → displacement = 6/256 = 0.0234375
        Assert.AreEqual(6f / 256f, dispSpace, 0.0001f);
    }

    [TestMethod]
    public void NonSpacingAccent_DoesNotAdvancePen()
    {
        var state = new NaplpsState();
        var penBefore = state.Pen;

        // Acute accent (U+00B4) — non-spacing from supplementary set
        var command = new AsciiCharCommand('\u00B4', state, 0x42, new NaplpsOperands([]));

        Assert.IsTrue(command.IsNonSpacing);
        Assert.AreEqual(penBefore.X, state.Pen.X);
        Assert.AreEqual(penBefore.Y, state.Pen.Y);
    }

    [TestMethod]
    public void RegularChar_AdvancesPen()
    {
        var state = new NaplpsState();
        var penBefore = state.Pen;

        var command = new AsciiCharCommand('A', state, 0x41, new NaplpsOperands([]));

        Assert.IsFalse(command.IsNonSpacing);
        Assert.AreNotEqual(penBefore.X, state.Pen.X);
    }
}
