// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Commands;

[TestClass]
public class ResetCommandTests
{
    [TestMethod]
    public void NoOperands_AllFlagsOff()
    {
        var command = new ResetCommand(new(), 0xA0, new NaplpsOperands([]));

        Assert.IsFalse(command.IsDomainReset);
        Assert.IsFalse(command.IsTextReset);
        Assert.IsFalse(command.IsBlinkReset);
        Assert.IsFalse(command.IsProtectedFields);
        Assert.IsFalse(command.IsTextureAttributesReset);
        Assert.IsFalse(command.IsMacrosReset);
        Assert.IsFalse(command.IsDRCSCharsReset);
        Assert.AreEqual(ResetCommand.ColorModeReset.NoAction, command.ColorMode);
        Assert.AreEqual(ResetCommand.ScreenBorderReset.NoAction, command.ColorScreenBorder);
    }

    [TestMethod]
    public void DomainReset_Bit1()
    {
        // Byte 1 bit 1 = 1: domain reset. Encoded as 0x41 (bit 1 set + header bit 7)
        var command = new ResetCommand(new(), 0xA0, new NaplpsOperands([0x41, 0x40]));

        Assert.IsTrue(command.IsDomainReset);
    }

    [TestMethod]
    public void ScreenClearBlack()
    {
        // Byte 1 bits 6,5,4 = 0,0,1: clear screen to black. = 0x48
        var command = new ResetCommand(new(), 0xA0, new NaplpsOperands([0x48, 0x40]));

        Assert.AreEqual(ResetCommand.ScreenBorderReset.ScreenBlack, command.ColorScreenBorder);
    }

    [TestMethod]
    public void TextAndBlinkReset()
    {
        // Byte 2 bit 1 = text reset, bit 2 = blink reset. = 0x43
        var command = new ResetCommand(new(), 0xA0, new NaplpsOperands([0x40, 0x43]));

        Assert.IsTrue(command.IsTextReset);
        Assert.IsTrue(command.IsBlinkReset);
    }
}
