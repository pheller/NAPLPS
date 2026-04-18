// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Numerics;

namespace NAPLPSTests.Base;

/// <summary>
/// ANSI X3.110 §5.3.2.1: a non-spacing accent (G2 supplementary set positions 0x40-0x4F)
/// is composed visually onto the FOLLOWING base character at the same field position,
/// without advancing the cursor on the accent itself.
///
/// Pre-existing behavior: non-spacing accents were detected (IsNonSpacing) and pen-advance
/// was suppressed, but each glyph rendered at its own pen position — accent BEFORE base —
/// producing two separate glyphs instead of a composite.
///
/// New behavior: the accent waits in NaplpsState.PendingAccentChar; the next spacing char
/// captures it into AsciiCharCommand.OverlayAccent and the renderer paints both at the
/// same field origin.
/// </summary>
[TestClass]
public class G2AccentCompositionTests
{
    private static NaplpsState CreateState()
    {
        var state = new NaplpsState();
        state.Pen = new Vector3(0.1f, 0.5f, 0);
        state.CharSize = new Vector2(1.0f / 40.0f, 5.0f / 128.0f);
        return state;
    }

    [TestMethod]
    public void NonSpacingAccent_PopulatesPendingAccentChar()
    {
        var state = CreateState();
        Assert.IsNull(state.PendingAccentChar);

        // 0xB4 (´ acute accent) is in the non-spacing set.
        var accent = new AsciiCharCommand('\u00B4', state, 0xC2, new NaplpsOperands([]));

        Assert.IsTrue(accent.IsNonSpacing, "accent must be flagged non-spacing");
        Assert.AreEqual('\u00B4', state.PendingAccentChar, "pending accent must be set on state");
    }

    [TestMethod]
    public void NextSpacingChar_CapturesAndClearsPendingAccent()
    {
        var state = CreateState();
        new AsciiCharCommand('\u00B4', state, 0xC2, new NaplpsOperands([]));
        Assert.AreEqual('\u00B4', state.PendingAccentChar);

        var baseChar = new AsciiCharCommand('e', state, 0x65, new NaplpsOperands([]));

        Assert.AreEqual('\u00B4', baseChar.OverlayAccent, "base char must capture the pending accent");
        Assert.IsNull(state.PendingAccentChar, "pending accent must be cleared after capture");
        Assert.IsFalse(baseChar.IsNonSpacing, "base char must not be marked non-spacing");
    }

    [TestMethod]
    public void NonSpacingAccent_DoesNotAdvancePen()
    {
        var state = CreateState();
        var penBefore = state.Pen.X;

        new AsciiCharCommand('\u00B4', state, 0xC2, new NaplpsOperands([]));

        Assert.AreEqual(penBefore, state.Pen.X, "non-spacing accent must not advance pen");
    }

    [TestMethod]
    public void BaseChar_AdvancesPenNormallyAfterAccent()
    {
        var state = CreateState();
        new AsciiCharCommand('\u00B4', state, 0xC2, new NaplpsOperands([]));
        var penAfterAccent = state.Pen.X;

        new AsciiCharCommand('e', state, 0x65, new NaplpsOperands([]));

        Assert.IsTrue(state.Pen.X > penAfterAccent, "base char after accent must still advance pen normally");
    }

    [TestMethod]
    public void TwoAccentsThenBase_OnlyLastAccentComposed()
    {
        // Spec edge case: if a second accent arrives without a base in between, only the
        // most recent one wins. (NAPLPS doesn't support stacked diacritics.)
        var state = CreateState();
        new AsciiCharCommand('\u00B4', state, 0xC2, new NaplpsOperands([])); // acute
        new AsciiCharCommand('\u00A8', state, 0xC8, new NaplpsOperands([])); // diaeresis

        Assert.AreEqual('\u00A8', state.PendingAccentChar, "second accent must overwrite first");

        var baseChar = new AsciiCharCommand('a', state, 0x61, new NaplpsOperands([]));
        Assert.AreEqual('\u00A8', baseChar.OverlayAccent);
    }

    [TestMethod]
    public void RegularChar_HasNoOverlayAccent()
    {
        var state = CreateState();
        var c = new AsciiCharCommand('a', state, 0x61, new NaplpsOperands([]));
        Assert.IsNull(c.OverlayAccent);
    }

    [TestMethod]
    public void Clone_DoesNotPersistPendingAccent()
    {
        // PendingAccentChar is JsonIgnore'd so it doesn't bleed across cloned states —
        // accent composition is a parse-time pen-advance concern, not a serialized one.
        var state = CreateState();
        state.PendingAccentChar = '\u00B4';

        var clone = state.Clone();

        Assert.IsNull(clone.PendingAccentChar);
    }
}
