// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS.Commands;

namespace NAPLPSTests.Commands;

[TestClass]
public class ShiftInCommandTests
{
    [TestMethod]
    public void Defaults()
    {
        var shiftInCommand = new ShiftInCommand(new(), []);

        Assert.IsNotNull(shiftInCommand);

        Assert.IsTrue(shiftInCommand.IsValid);
    }

    /// <summary>
    /// Based on https://archive.org/details/byte-magazine-1983-03/page/n163/mode/1up?view=theater
    /// </summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page163Road()
    {
        const string asciiString = "ROAD";

        var shiftInCommand = new ShiftInCommand(new(), [0x52, 0x4F, 0x41, 0x44]);

        Assert.IsNotNull(shiftInCommand);

        Assert.IsTrue(shiftInCommand.IsValid);

        Assert.AreEqual(shiftInCommand.Text.Length, asciiString.Length);
        Assert.AreEqual(shiftInCommand.Text, asciiString);
    }

    /// <summary>
    /// Based on https://archive.org/details/byte-magazine-1983-03/page/n164/mode/1up?view=theater
    /// </summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page164Figure1()
    {
        const string asciiString = "Figure 1";

        var shiftInCommand = new ShiftInCommand(new(), [0x46, 0x69, 0x67, 0x75, 0x72, 0x65, 0x20, 0x31]);

        Assert.IsNotNull(shiftInCommand);

        Assert.IsTrue(shiftInCommand.IsValid);

        Assert.AreEqual(shiftInCommand.Text.Length, asciiString.Length);
        Assert.AreEqual(shiftInCommand.Text, asciiString);
    }
}
