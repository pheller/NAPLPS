// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Commands;

[TestClass]
public class SelectColorCommandTests
{
    [TestMethod]
    public void Defaults()
    {
        var command = new SelectColorCommand(new(), 0xBE, new NaplpsOperands([]));

        Assert.IsNotNull(command);
        Assert.AreEqual(0, command.State.ColorMode);
    }

    [TestMethod]
    public void TestSlot1()
    {
        var command = new SelectColorCommand(new(), 0xBE, new NaplpsOperands([0x44]));

        Assert.IsNotNull(command);

        Assert.AreEqual(1, command.State.ColorMode);
        Assert.AreEqual(1, command.State.ColorMapForeground);
    }

    [TestMethod]
    public void TestSlot2()
    {
        var command = new SelectColorCommand(new(), 0xBE, new NaplpsOperands([0x48]));

        Assert.IsNotNull(command);

        Assert.AreEqual(1, command.State.ColorMode);
        Assert.AreEqual(2, command.State.ColorMapForeground);
    }

    [TestMethod]
    public void TestSlot3()
    {
        var command = new SelectColorCommand(new(), 0xBE, new NaplpsOperands([0x4C]));

        Assert.IsNotNull(command);

        Assert.AreEqual(1, command.State.ColorMode);
        Assert.AreEqual(3, command.State.ColorMapForeground);
    }

    [TestMethod]
    public void TestSlot4()
    {
        var command = new SelectColorCommand(new(), 0xBE, new NaplpsOperands([0x50]));

        Assert.IsNotNull(command);

        Assert.AreEqual(1, command.State.ColorMode);
        Assert.AreEqual(4, command.State.ColorMapForeground);
    }

    [TestMethod]
    public void TestSlot5()
    {
        var command = new SelectColorCommand(new(), 0xBE, new NaplpsOperands([0x54]));

        Assert.IsNotNull(command);

        Assert.AreEqual(1, command.State.ColorMode);
        Assert.AreEqual(5, command.State.ColorMapForeground);
    }
}
