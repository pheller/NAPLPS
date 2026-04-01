// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using static NAPLPS.NaplpsTexture;

namespace NAPLPSTests.Commands;

[TestClass]
public class TextureCommandTests
{
    // ANSI X3.110: TEXTURE operand byte layout (7-bit mode, data in bits 6-1):
    //   Bits 6,5,4 → texture pattern (0-7), bit 6 = MSB
    //   Bit 3      → highlight flag
    //   Bits 2,1   → line texture (0-3), bit 2 = MSB
    //
    // Byte 0x40 = all data bits zero → Solid, no highlight, Solid line

    private static TextureCommand CreateTexture(byte operandByte)
    {
        return new TextureCommand(new(), 0xA3, new NaplpsOperands([operandByte]));
    }

    // ========================================================================
    // Texture Pattern bit ordering (bits 6,5,4 → MSB to LSB)
    // ========================================================================

    [TestMethod]
    public void TexturePattern_Solid()
    {
        // 0x40 = 01_000_0_00 → pattern bits = 000 = 0 = Solid
        var cmd = CreateTexture(0x40);

        Assert.AreEqual(TexturePatterns.Solid, cmd.TexturePattern);
    }

    [TestMethod]
    public void TexturePattern_VerticalHatching()
    {
        // Pattern 1 = 001 → bit6=0, bit5=0, bit4=1
        // Byte: 01_001_0_00 = 0x48
        var cmd = CreateTexture(0x48);

        Assert.AreEqual(TexturePatterns.VerticalHatching, cmd.TexturePattern);
    }

    [TestMethod]
    public void TexturePattern_HorizontalHatching()
    {
        // Pattern 2 = 010 → bit6=0, bit5=1, bit4=0
        // Byte: 01_010_0_00 = 0x50
        var cmd = CreateTexture(0x50);

        Assert.AreEqual(TexturePatterns.HorizontalHatching, cmd.TexturePattern);
    }

    [TestMethod]
    public void TexturePattern_CrossHatching()
    {
        // Pattern 3 = 011 → bit6=0, bit5=1, bit4=1
        // Byte: 01_011_0_00 = 0x58
        var cmd = CreateTexture(0x58);

        Assert.AreEqual(TexturePatterns.CrossHatching, cmd.TexturePattern);
    }

    [TestMethod]
    public void TexturePattern_MaskA()
    {
        // Pattern 4 = 100 → bit6=1, bit5=0, bit4=0
        // Byte: 01_100_0_00 = 0x60
        var cmd = CreateTexture(0x60);

        Assert.AreEqual(TexturePatterns.MaskA, cmd.TexturePattern);
    }

    [TestMethod]
    public void TexturePattern_MaskB()
    {
        // Pattern 5 = 101 → bit6=1, bit5=0, bit4=1
        // Byte: 01_101_0_00 = 0x68
        var cmd = CreateTexture(0x68);

        Assert.AreEqual(TexturePatterns.MaskB, cmd.TexturePattern);
    }

    [TestMethod]
    public void TexturePattern_MaskC()
    {
        // Pattern 6 = 110 → bit6=1, bit5=1, bit4=0
        // Byte: 01_110_0_00 = 0x70
        var cmd = CreateTexture(0x70);

        Assert.AreEqual(TexturePatterns.MaskC, cmd.TexturePattern);
    }

    [TestMethod]
    public void TexturePattern_MaskD()
    {
        // Pattern 7 = 111 → bit6=1, bit5=1, bit4=1
        // Byte: 01_111_0_00 = 0x78
        var cmd = CreateTexture(0x78);

        Assert.AreEqual(TexturePatterns.MaskD, cmd.TexturePattern);
    }

    // ========================================================================
    // Highlight flag (bit 3)
    // ========================================================================

    [TestMethod]
    public void Highlight_Off()
    {
        // 0x40 = bit3=0
        var cmd = CreateTexture(0x40);

        Assert.IsFalse(cmd.ShouldHighlight);
    }

    [TestMethod]
    public void Highlight_On()
    {
        // 0x44 = 01_000_1_00 → bit3=1, Solid pattern + highlight
        var cmd = CreateTexture(0x44);

        Assert.IsTrue(cmd.ShouldHighlight);
    }

    [TestMethod]
    public void Highlight_WithPattern()
    {
        // 0x54 = 01_010_1_00 → HorizontalHatching + highlight
        var cmd = CreateTexture(0x54);

        Assert.AreEqual(TexturePatterns.HorizontalHatching, cmd.TexturePattern);
        Assert.IsTrue(cmd.ShouldHighlight);
    }

    // ========================================================================
    // Line texture (bits 2,1)
    // ========================================================================

    [TestMethod]
    public void LineTexture_Solid()
    {
        // 0x40 = bits 2,1 = 00 → Solid
        var cmd = CreateTexture(0x40);

        Assert.AreEqual(LineTextures.Solid, cmd.LineTexture);
    }

    [TestMethod]
    public void LineTexture_Dotted()
    {
        // NAPLPS 1-indexed: bit1=LSB(0x01), bit2=MSB(0x02) of line texture, bit3=highlight(0x04)
        // Dotted = 1 = bit2=0, bit1=1 → byte: 0x40 | 0x01 = 0x41
        var cmd = CreateTexture(0x41);

        Assert.AreEqual(LineTextures.Dotted, cmd.LineTexture);
    }

    [TestMethod]
    public void LineTexture_Dashed()
    {
        // Dashed = 2 = bit2=1, bit1=0 → byte: 0x40 | 0x02 = 0x42
        var cmd = CreateTexture(0x42);

        Assert.AreEqual(LineTextures.Dashed, cmd.LineTexture);
        Assert.IsFalse(cmd.ShouldHighlight);
    }

    [TestMethod]
    public void LineTexture_DottedDashed()
    {
        // DottedDashed = 3 = bit2=1, bit1=1 → 0x02 | 0x01 = 0x03
        var cmd = CreateTexture(0x43); // 0x40 | 0x03

        Assert.AreEqual(LineTextures.DottedDashed, cmd.LineTexture);
    }

    // ========================================================================
    // Combined fields
    // ========================================================================

    [TestMethod]
    public void AllFieldsCombined()
    {
        // CrossHatching(3) + highlight + Dashed(2)
        // Pattern 3 = 011 → bit6=0, bit5=1, bit4=1 → 0x10 | 0x08 = 0x18
        // Highlight = bit3 → 0x04
        // Dashed = 2 → bit2=1 → 0x02
        // Byte = 0x40 | 0x18 | 0x04 | 0x02 = 0x5E
        var cmd = CreateTexture(0x5E);

        Assert.AreEqual(TexturePatterns.CrossHatching, cmd.TexturePattern);
        Assert.IsTrue(cmd.ShouldHighlight);
        Assert.AreEqual(LineTextures.Dashed, cmd.LineTexture);
    }

    // ========================================================================
    // State propagation
    // ========================================================================

    [TestMethod]
    public void TextureCommand_SetsStateTexture()
    {
        var state = new NaplpsState();

        Assert.AreEqual(TexturePatterns.Solid, state.Texture.TexturePattern);

        var cmd = new TextureCommand(state, 0xA3, new NaplpsOperands([0x50])); // HorizontalHatching

        Assert.AreEqual(TexturePatterns.HorizontalHatching, state.Texture.TexturePattern);
    }

    [TestMethod]
    public void NoOperands_DefaultsToSolid()
    {
        var cmd = new TextureCommand(new(), 0xA3, new NaplpsOperands([]));

        Assert.AreEqual(TexturePatterns.Solid, cmd.TexturePattern);
        Assert.IsFalse(cmd.ShouldHighlight);
        Assert.AreEqual(LineTextures.Solid, cmd.LineTexture);
    }
}
