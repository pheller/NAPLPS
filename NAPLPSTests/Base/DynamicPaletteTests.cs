// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Base;

/// <summary>
/// ANSI X3.110 §5.2.1: systems capable of more than 16 palette entries should
/// generate the extended palette algorithmically (greyscale ramp + hue-circle).
/// These tests verify the wiring from system capability through to the active ColorMap.
/// </summary>
[TestClass]
public class DynamicPaletteTests
{
    [TestMethod]
    public void DefaultState_Has16ColorCapacity()
    {
        var state = new NaplpsState();
        Assert.AreEqual(16, state.ColorCapacity);
        Assert.AreEqual(16, state.ColorMap.Count);
    }

    [TestMethod]
    public void State256_GeneratesFullPalette()
    {
        var state = new NaplpsState(256);
        Assert.AreEqual(256, state.ColorCapacity);
        Assert.AreEqual(256, state.ColorMap.Count);
    }

    [TestMethod]
    public void State64_GeneratesFullPalette()
    {
        var state = new NaplpsState(64);
        Assert.AreEqual(64, state.ColorCapacity);
        Assert.AreEqual(64, state.ColorMap.Count);
    }

    [TestMethod]
    public void State256_FirstHalfIsGreyscale()
    {
        var state = new NaplpsState(256);
        // Spec §5.2.1.2: first half of generated palette is uniformly spaced greyscale.
        for (byte i = 0; i < 128; i++)
        {
            var c = state.ColorMap[i];
            Assert.AreEqual(c.Red, c.Green, $"entry {i} not grey: R={c.Red} G={c.Green}");
            Assert.AreEqual(c.Green, c.Blue, $"entry {i} not grey: G={c.Green} B={c.Blue}");
        }
    }

    [TestMethod]
    public void State256_PaletteIndex200Addressable()
    {
        var state = new NaplpsState(256);
        // index 200 is in extended range — would clamp/fail on default 16-entry state.
        Assert.IsTrue(state.ColorMap.ContainsKey(200), "extended palette must contain entry 200");
    }

    [TestMethod]
    public void NaplpsFormatNew_ThreadsColorCapacity()
    {
        var fmt = NaplpsFormat.New(NaplpsSystemType.NAPLPS, colorCapacity: 256);
        Assert.AreEqual(256, fmt.State.ColorCapacity);
        Assert.AreEqual(256, fmt.State.ColorMap.Count);
    }

    [TestMethod]
    public void DefaultNaplpsFormatNew_StaysAt16()
    {
        var fmt = NaplpsFormat.New();
        Assert.AreEqual(16, fmt.State.ColorCapacity);
        Assert.AreEqual(16, fmt.State.ColorMap.Count);
    }

    [TestMethod]
    public void Clone_PreservesColorCapacity()
    {
        var state = new NaplpsState(256);
        var clone = state.Clone();
        Assert.AreEqual(256, clone.ColorCapacity);
        Assert.AreEqual(256, clone.ColorMap.Count);
    }
}
