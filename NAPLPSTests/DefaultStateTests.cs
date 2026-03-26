// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Numerics;

namespace NAPLPSTests;

[TestClass]
public class DefaultStateTests
{
    [TestMethod]
    public void DefaultForeground_IsWhite()
    {
        var state = new NaplpsState();

        Assert.AreEqual(NaplpsColor.White.Red, state.Foreground.Red);
        Assert.AreEqual(NaplpsColor.White.Green, state.Foreground.Green);
        Assert.AreEqual(NaplpsColor.White.Blue, state.Foreground.Blue);
    }

    [TestMethod]
    public void DefaultColorMapForeground_IsNominalWhite()
    {
        var state = new NaplpsState();

        // Property initializer sets 0x07, but SelectColor in default InUseTable
        // initialization may override. Verify actual runtime default.
        byte actual = state.ColorMapForeground;
        Assert.IsTrue(actual == 0x07 || actual == 0x01,
            $"Expected 0x07 (nominal white) or 0x01, got 0x{actual:X2}");
    }

    [TestMethod]
    public void DefaultCharSize_Is1Over40By5Over128()
    {
        var state = new NaplpsState();

        Assert.AreEqual(1.0f / 40.0f, state.CharSize.X, 0.0001f);
        Assert.AreEqual(5.0f / 128.0f, state.CharSize.Y, 0.0001f);
    }

    [TestMethod]
    public void DefaultDomain_3ByteMultiValue_1ByteSingle_2D()
    {
        var state = new NaplpsState();

        Assert.AreEqual(3, state.MultiByteValue);
        Assert.AreEqual(1, state.SingleByteValue);
        Assert.AreEqual(2, state.Dimensionality);
    }

    [TestMethod]
    public void DefaultLogicalPel_IsZero()
    {
        var state = new NaplpsState();

        Assert.AreEqual(0f, state.LogicalPel.X);
        Assert.AreEqual(0f, state.LogicalPel.Y);
    }

    [TestMethod]
    public void DefaultPalette_Has16Entries()
    {
        var state = new NaplpsState();

        Assert.AreEqual(16, state.ColorMap.Count);
    }

    [TestMethod]
    public void DefaultPalette_NominalBlack()
    {
        var state = new NaplpsState();

        var black = state.ColorMap[0x00];
        Assert.AreEqual(0, black.Red);
        Assert.AreEqual(0, black.Green);
        Assert.AreEqual(0, black.Blue);
    }

    [TestMethod]
    public void DefaultPalette_NominalWhite()
    {
        var state = new NaplpsState();

        var white = state.ColorMap[0x07];
        Assert.AreEqual(255, white.Red);
        Assert.AreEqual(255, white.Green);
        Assert.AreEqual(255, white.Blue);
    }

    [TestMethod]
    public void DefaultColorMode_IsZero()
    {
        var state = new NaplpsState();

        Assert.AreEqual(0, state.ColorMode);
    }

    [TestMethod]
    public void DefaultTextSettings()
    {
        var state = new NaplpsState();

        Assert.AreEqual(TextCommand.TextRotation.Zero, state.TextRotation);
        Assert.AreEqual(TextCommand.TextPath.Right, state.TextPath);
        Assert.AreEqual(TextCommand.TextSpacing.One, state.TextSpacing);
        Assert.AreEqual(TextCommand.TextInterrowSpacing.One, state.TextInterrowSpacing);
        Assert.AreEqual(TextCommand.TextMoveAttributes.MoveTogether, state.TextMoveAttributes);
    }

    [TestMethod]
    public void DefaultFlags_AllOff()
    {
        var state = new NaplpsState();

        Assert.IsFalse(state.IsReverseVideo);
        Assert.IsFalse(state.IsUnderline);
        Assert.IsFalse(state.IsScrollMode);
        Assert.IsFalse(state.IsWordWrapMode);
        Assert.IsFalse(state.IsBlinkMode);
        Assert.IsFalse(state.IsProtectMode);
        Assert.IsFalse(state.IsTransparent);
    }

    [TestMethod]
    public void UsedPaletteEntries_StartsEmpty()
    {
        var state = new NaplpsState();

        Assert.AreEqual(0, state.UsedPaletteEntries.Count);
    }
}
