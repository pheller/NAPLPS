// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Commands;

[TestClass]
public class SelectColorCommandTests
{
    [TestMethod]
    public void Defaults()
    {
        var command = new SelectColorCommand(new(), []);

        Assert.IsNotNull(command);
        Assert.AreEqual(command.State.ColorMode, 0);
    }

    [TestMethod]
    public void TestSlot1()
    {
        var command = new SelectColorCommand(new(), [0x44]);

        Assert.IsNotNull(command);
        
        Assert.AreEqual(command.State.ColorMode, 1);
        Assert.AreEqual(command.State.ColorMapForeground, 1);
    }

    [TestMethod]
    public void TestSlot2()
    {
        var command = new SelectColorCommand(new(), [0x48]);

        Assert.IsNotNull(command);

        Assert.AreEqual(command.State.ColorMode, 1);
        Assert.AreEqual(command.State.ColorMapForeground, 2);
    }

    [TestMethod]
    public void TestSlot3()
    {
        var command = new SelectColorCommand(new(), [0x4C]);

        Assert.IsNotNull(command);

        Assert.AreEqual(command.State.ColorMode, 1);
        Assert.AreEqual(command.State.ColorMapForeground, 3);
    }

    [TestMethod]
    public void TestSlot4()
    {
        var command = new SelectColorCommand(new(), [0x50]);

        Assert.IsNotNull(command);

        Assert.AreEqual(command.State.ColorMode, 1);
        Assert.AreEqual(command.State.ColorMapForeground, 4);
    }

    [TestMethod]
    public void TestSlot5()
    {
        var command = new SelectColorCommand(new(), [0x54]);

        Assert.IsNotNull(command);

        Assert.AreEqual(command.State.ColorMode, 1);
        Assert.AreEqual(command.State.ColorMapForeground, 5);
    }
}
