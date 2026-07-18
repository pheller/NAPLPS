// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Numerics;

namespace NAPLPSTests.Base;

/// <summary>
/// ANSI X3.110 5.3.2.1: a non-spacing accent (G2 supplementary set positions 0x40-0x4F) is
/// composed visually onto the FOLLOWING base character at the same field position, without
/// advancing the cursor on the accent itself.
///
/// The accent waits in NaplpsState.PendingAccentChar/Code; the next spacing char captures it
/// into AsciiCharCommand.OverlayAccent(Code) and the MVDI renderer overlays both glyphs at the
/// same field origin. Non-spacing status is tagged from the actual G2 invocation (SS2 or a
/// locking shift) via NaplpsState.ResolveByte, not inferred from the Unicode codepoint, so
/// these tests drive the accent through that path exactly as the parser does.
/// </summary>
[TestClass]
public class G2AccentCompositionTests
{
    private const char Acute = '\u00B4';       // G2 code 0x42 (acute accent)
    private const char Diaeresis = '\u00A8';   // G2 code 0x48 (diaeresis)

    private static NaplpsState CreateState()
    {
        var state = new NaplpsState();
        state.Pen = new Vector3(0.1f, 0.5f, 0);
        state.CharSize = new Vector2(1.0f / 40.0f, 5.0f / 128.0f);
        return state;
    }

    /// <summary>Resolve a byte from the G2 supplementary set via SS2, exactly as the parser
    /// does, so the resulting command is tagged IsSupplementary with the right G2 code.</summary>
    private static AsciiCharCommand SupplementaryChar(NaplpsState state, char ch, byte opcode)
    {
        state.DoSingleShiftTwo();
        state.ResolveByte(opcode);
        return new AsciiCharCommand(ch, state, opcode, new NaplpsOperands([]));
    }

    /// <summary>Resolve a primary-set byte (clears any stale supplementary tag).</summary>
    private static AsciiCharCommand PrimaryChar(NaplpsState state, char ch, byte opcode)
    {
        state.ResolveByte(opcode);
        return new AsciiCharCommand(ch, state, opcode, new NaplpsOperands([]));
    }

    [TestMethod]
    public void NonSpacingAccent_PopulatesPendingAccentChar()
    {
        var state = CreateState();
        Assert.IsNull(state.PendingAccentChar);

        // 0xC2 -> G2 code 0x42 (acute accent), in the non-spacing column 0x40-0x4F.
        var accent = SupplementaryChar(state, Acute, 0xC2);

        Assert.IsTrue(accent.IsNonSpacing, "accent must be flagged non-spacing");
        Assert.AreEqual(Acute, state.PendingAccentChar, "pending accent must be set on state");
    }

    [TestMethod]
    public void NextSpacingChar_CapturesAndClearsPendingAccent()
    {
        var state = CreateState();
        SupplementaryChar(state, Acute, 0xC2);
        Assert.AreEqual(Acute, state.PendingAccentChar);

        var baseChar = PrimaryChar(state, 'e', 0x65);

        Assert.AreEqual(Acute, baseChar.OverlayAccent, "base char must capture the pending accent");
        Assert.IsNull(state.PendingAccentChar, "pending accent must be cleared after capture");
        Assert.IsFalse(baseChar.IsNonSpacing, "base char must not be marked non-spacing");
    }

    [TestMethod]
    public void NonSpacingAccent_DoesNotAdvancePen()
    {
        var state = CreateState();
        var penBefore = state.Pen.X;

        SupplementaryChar(state, Acute, 0xC2);

        Assert.AreEqual(penBefore, state.Pen.X, "non-spacing accent must not advance pen");
    }

    [TestMethod]
    public void BaseChar_AdvancesPenNormallyAfterAccent()
    {
        var state = CreateState();
        SupplementaryChar(state, Acute, 0xC2);
        var penAfterAccent = state.Pen.X;

        PrimaryChar(state, 'e', 0x65);

        Assert.IsTrue(state.Pen.X > penAfterAccent, "base char after accent must still advance pen normally");
    }

    [TestMethod]
    public void TwoAccentsThenBase_OnlyLastAccentComposed()
    {
        // Spec edge case: if a second accent arrives without a base in between, only the
        // most recent one wins. (NAPLPS doesn't support stacked diacritics.)
        var state = CreateState();
        SupplementaryChar(state, Acute, 0xC2);      // acute (G2 0x42)
        SupplementaryChar(state, Diaeresis, 0xC8);  // diaeresis (G2 0x48)

        Assert.AreEqual(Diaeresis, state.PendingAccentChar, "second accent must overwrite first");

        var baseChar = PrimaryChar(state, 'a', 0x61);
        Assert.AreEqual(Diaeresis, baseChar.OverlayAccent);
    }

    [TestMethod]
    public void RegularChar_HasNoOverlayAccent()
    {
        var state = CreateState();
        var c = PrimaryChar(state, 'a', 0x61);
        Assert.IsNull(c.OverlayAccent);
    }

    [TestMethod]
    public void Clone_DoesNotPersistPendingAccent()
    {
        // PendingAccentChar is JsonIgnore'd so it doesn't bleed across cloned states -
        // accent composition is a parse-time pen-advance concern, not a serialized one.
        var state = CreateState();
        state.PendingAccentChar = Acute;

        var clone = state.Clone();

        Assert.IsNull(clone.PendingAccentChar);
    }
}
