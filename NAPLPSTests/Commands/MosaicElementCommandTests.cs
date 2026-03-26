// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Commands;

[TestClass]
public class MosaicElementCommandTests
{
    [TestMethod]
    public void AllBitsSet()
    {
        var bits = new bool[] { true, true, true, true, true, true };
        var command = new MosaicElementCommand(bits, new(), 0x7F, new NaplpsOperands([]));

        Assert.IsTrue(command.Bit1);
        Assert.IsTrue(command.Bit2);
        Assert.IsTrue(command.Bit3);
        Assert.IsTrue(command.Bit4);
        Assert.IsTrue(command.Bit5);
        Assert.IsTrue(command.Bit6);
    }

    [TestMethod]
    public void NoBitsSet()
    {
        var bits = new bool[] { false, false, false, false, false, false };
        var command = new MosaicElementCommand(bits, new(), 0x60, new NaplpsOperands([]));

        Assert.IsFalse(command.Bit1);
        Assert.IsFalse(command.Bit2);
        Assert.IsFalse(command.Bit3);
        Assert.IsFalse(command.Bit4);
        Assert.IsFalse(command.Bit5);
        Assert.IsFalse(command.Bit6);
    }

    [TestMethod]
    public void Checkerboard()
    {
        var bits = new bool[] { true, false, true, false, true, false };
        var command = new MosaicElementCommand(bits, new(), 0x55, new NaplpsOperands([]));

        Assert.IsTrue(command.Bit1);
        Assert.IsFalse(command.Bit2);
        Assert.IsTrue(command.Bit3);
        Assert.IsFalse(command.Bit4);
        Assert.IsTrue(command.Bit5);
        Assert.IsFalse(command.Bit6);
    }

    [TestMethod]
    public void WrongLength_Throws()
    {
        try
        {
            new MosaicElementCommand(new bool[] { true, false }, new(), 0x40, new NaplpsOperands([]));
            Assert.Fail("Should have thrown ArgumentException");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }
}
