// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Commands;

[TestClass]
public class SetColorMode0PaletteTests
{
    [TestMethod]
    public void Mode0_DirectColor_FindsExistingPaletteEntry()
    {
        var state = new NaplpsState();
        // Ensure we're in color mode 0 and palette has white at 0x07
        state.ColorMode = 0;

        // Set foreground to white via SetColor operands
        var command = new SetColorCommand(state, 0xBC, new NaplpsOperands([0x7F, 0x7F, 0x7F]));

        // Should have found white in palette and set foreground index accordingly
        var fgColor = state.ColorMap[state.ColorMapForeground];
        Assert.AreEqual(state.Foreground.Red, fgColor.Red);
        Assert.AreEqual(state.Foreground.Green, fgColor.Green);
        Assert.AreEqual(state.Foreground.Blue, fgColor.Blue);
    }

    [TestMethod]
    public void Mode0_NewColor_AllocatesUnusedEntry()
    {
        var state = new NaplpsState();

        // Set a unique color that doesn't exist in default palette
        var command = new SetColorCommand(state, 0xBC, new NaplpsOperands([0x55, 0x55, 0x55]));

        // Should have allocated a palette entry (not 0x00 or 0x07)
        Assert.AreNotEqual(0x00, state.ColorMapForeground);
        Assert.AreNotEqual(0x07, state.ColorMapForeground);
        Assert.IsTrue(state.UsedPaletteEntries.Contains(state.ColorMapForeground));
    }

    [TestMethod]
    public void Mode0_NoOperands_SetsTransparent()
    {
        var state = new NaplpsState();

        var command = new SetColorCommand(state, 0xBC, new NaplpsOperands([]));

        Assert.IsTrue(state.IsTransparent);
    }
}
