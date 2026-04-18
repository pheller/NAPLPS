// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Numerics;

namespace NAPLPSTests.Commands;

[TestClass]
public class WordWrapTests
{
    private static NaplpsState CreateFieldState(float fieldWidth = 0.5f, float fieldHeight = 0.5f)
    {
        var state = new NaplpsState();
        state.Field = new NaplpsField(new Vector3(0, 0, 0), new Vector3(fieldWidth, fieldHeight, 0));
        state.Pen = new Vector3(0, fieldHeight, 0);
        state.CharSize = new Vector2(1.0f / 40.0f, 5.0f / 128.0f);
        return state;
    }

    [TestMethod]
    public void WordBreakChars_AreRecognized()
    {
        Assert.IsTrue(AsciiCharCommand.IsWordBreakChar(' '));
        Assert.IsTrue(AsciiCharCommand.IsWordBreakChar('!'));
        Assert.IsTrue(AsciiCharCommand.IsWordBreakChar('-'));
        Assert.IsTrue(AsciiCharCommand.IsWordBreakChar(','));
        Assert.IsTrue(AsciiCharCommand.IsWordBreakChar('.'));
        Assert.IsTrue(AsciiCharCommand.IsWordBreakChar('/'));
        Assert.IsTrue(AsciiCharCommand.IsWordBreakChar('('));
        Assert.IsTrue(AsciiCharCommand.IsWordBreakChar(')'));
    }

    [TestMethod]
    public void WordBreakChars_LettersAreNot()
    {
        Assert.IsFalse(AsciiCharCommand.IsWordBreakChar('A'));
        Assert.IsFalse(AsciiCharCommand.IsWordBreakChar('z'));
        Assert.IsFalse(AsciiCharCommand.IsWordBreakChar('0'));
    }

    [TestMethod]
    public void AutoWrap_PenReturnsToFieldOriginX()
    {
        // Field width 0.1, char width 0.025, wrap tolerance is 3x char width = 0.075.
        // Threshold for wrap = fieldRight (0.1) + 0.075 = 0.175. Pen starts at 0.16
        // so a char advance to 0.185 crosses the threshold.
        var state = CreateFieldState(0.1f, 0.5f);
        state.Pen = new Vector3(0.16f, 0.4f, 0);

        var cmd = new AsciiCharCommand('X', state, 0x58, new NaplpsOperands([]));

        // Pen X should have wrapped to field origin
        Assert.IsTrue(state.Pen.X < 0.05f, $"Expected pen X near 0, got {state.Pen.X}");
    }

    [TestMethod]
    public void WordWrapOn_SpaceAtEndOfLine_IsDiscarded()
    {
        // Same threshold setup as AutoWrap_PenReturnsToFieldOriginX above.
        var state = CreateFieldState(0.1f, 0.5f);
        state.IsWordWrapMode = true;
        state.Pen = new Vector3(0.16f, 0.4f, 0);

        var cmd = new AsciiCharCommand(' ', state, 0x20, new NaplpsOperands([]));

        Assert.IsTrue(cmd.IsDiscarded);
    }

    [TestMethod]
    public void WordWrapOff_SpaceAtEndOfLine_IsNotDiscarded()
    {
        var state = CreateFieldState(0.1f, 0.5f);
        state.IsWordWrapMode = false;
        state.Pen = new Vector3(0.16f, 0.4f, 0);

        var cmd = new AsciiCharCommand(' ', state, 0x20, new NaplpsOperands([]));

        Assert.IsFalse(cmd.IsDiscarded);
    }

    [TestMethod]
    public void NormalChar_NotDiscarded()
    {
        var state = CreateFieldState();
        state.IsWordWrapMode = true;

        var cmd = new AsciiCharCommand('A', state, 0x41, new NaplpsOperands([]));

        Assert.IsFalse(cmd.IsDiscarded);
    }

    [TestMethod]
    public void CharInMiddleOfField_NoWrap()
    {
        var state = CreateFieldState();
        state.Pen = new Vector3(0.1f, 0.4f, 0); // Well within field

        float penXBefore = state.Pen.X;

        var cmd = new AsciiCharCommand('A', state, 0x41, new NaplpsOperands([]));

        // Pen should have advanced, not wrapped
        Assert.IsTrue(state.Pen.X > penXBefore);
    }

    [TestMethod]
    public void WordWrapMode_TracksLastBreakPoint()
    {
        var state = CreateFieldState();
        state.IsWordWrapMode = true;
        state.Pen = new Vector3(0.1f, 0.4f, 0);

        // Type "a b" — the space should record a break point
        new AsciiCharCommand('a', state, 0x61, new NaplpsOperands([]));
        new AsciiCharCommand(' ', state, 0x20, new NaplpsOperands([]));

        var breakPen = state.LastWordBreakPen;

        new AsciiCharCommand('b', state, 0x62, new NaplpsOperands([]));

        // Break point should be at the space position, not updated by 'b'
        Assert.AreEqual(breakPen.X, state.LastWordBreakPen.X);
    }

    [TestMethod]
    public void AutoWrap_PenMovesDownByInterrowSpacing()
    {
        // Pen at 0.16, advances to 0.185, crosses 0.175 wrap threshold.
        var state = CreateFieldState(0.1f, 0.5f);
        state.Pen = new Vector3(0.16f, 0.4f, 0);
        float penYBefore = state.Pen.Y;

        var cmd = new AsciiCharCommand('X', state, 0x58, new NaplpsOperands([]));

        // Y should have decreased by approximately CharSize.Y * interrow multiplier
        Assert.IsTrue(state.Pen.Y < penYBefore, $"Expected pen Y to decrease, was {penYBefore}, now {state.Pen.Y}");
    }

    /// <summary>
    /// ANSI X3.110 §5.3.2.3.6: hyphen permits a word break mid-word. Unlike space (which
    /// gets discarded at line-end), a hyphen is content — it must remain at the end of the
    /// current line when wrap happens after it. Verifies hyphen behaves as a word-break char
    /// AND is not subject to the trailing-space discard rule.
    /// </summary>
    [TestMethod]
    public void HyphenMidWord_UpdatesBreakPoint()
    {
        var state = CreateFieldState();
        state.IsWordWrapMode = true;
        state.Pen = new Vector3(0.1f, 0.4f, 0);

        new AsciiCharCommand('a', state, 0x61, new NaplpsOperands([]));
        var penBeforeHyphen = state.LastWordBreakPen.X;
        new AsciiCharCommand('-', state, 0x2D, new NaplpsOperands([]));
        var penAfterHyphen = state.LastWordBreakPen.X;

        // Hyphen must update the break-point anchor. Before the hyphen, LastWordBreakPen
        // is still the field origin (no prior break); after the hyphen it should be the
        // pen position immediately following the hyphen glyph.
        Assert.AreNotEqual(penBeforeHyphen, penAfterHyphen, "hyphen must update LastWordBreakPen");
        Assert.IsTrue(penAfterHyphen > 0.1f, $"break pen should be past field origin, got {penAfterHyphen}");
    }

    [TestMethod]
    public void HyphenAtLineEnd_NotDiscarded()
    {
        // Set up a wrap-triggering pen position. Spec: hyphen is content, not whitespace —
        // unlike trailing spaces, it must not be discarded at the wrap point.
        var state = CreateFieldState(0.1f, 0.5f);
        state.IsWordWrapMode = true;
        state.Pen = new Vector3(0.16f, 0.4f, 0);

        var cmd = new AsciiCharCommand('-', state, 0x2D, new NaplpsOperands([]));

        Assert.IsFalse(cmd.IsDiscarded, "hyphen must never be discarded by word wrap (spec: only spaces discard)");
    }

    [TestMethod]
    public void LetterAfterHyphen_DoesNotMoveBreakPoint()
    {
        var state = CreateFieldState();
        state.IsWordWrapMode = true;
        state.Pen = new Vector3(0.1f, 0.4f, 0);

        new AsciiCharCommand('m', state, 0x6D, new NaplpsOperands([]));
        new AsciiCharCommand('-', state, 0x2D, new NaplpsOperands([]));
        var hyphenAnchor = state.LastWordBreakPen.X;

        new AsciiCharCommand('i', state, 0x69, new NaplpsOperands([]));
        new AsciiCharCommand('n', state, 0x6E, new NaplpsOperands([]));

        // Subsequent letters must NOT update the break point — hyphen anchor stays put.
        Assert.AreEqual(hyphenAnchor, state.LastWordBreakPen.X);
    }
}
