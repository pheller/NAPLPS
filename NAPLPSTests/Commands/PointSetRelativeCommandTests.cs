// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Commands;

[TestClass]
public class PointSetRelativeCommandTests
{
    [TestMethod]
    public void Defaults()
    {
        var pointSetRelativeCommand = new PointSetRelativeCommand(new(), 0xA5, new NaplpsOperands([]));

        Assert.IsNotNull(pointSetRelativeCommand);

        Assert.AreEqual(0, pointSetRelativeCommand.Vertices.Count);

        Assert.AreEqual(0f, pointSetRelativeCommand.State.Pen.X);
        Assert.AreEqual(0f, pointSetRelativeCommand.State.Pen.Y);
        Assert.AreEqual(0f, pointSetRelativeCommand.State.Pen.Z);
    }

    /// <summary>Based on https://archive.org/details/byte-magazine-1983-03/page/n157/mode/1up?view=theater</summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page156()
    {
        var pointSetRelativeCommand = new PointSetRelativeCommand(new(), 0xA5, new NaplpsOperands([0x78, 0x44, 0x60]));

        Assert.IsNotNull(pointSetRelativeCommand);

        Assert.AreEqual(0, pointSetRelativeCommand.Vertices.Count);

        Assert.AreEqual(-.234375f, pointSetRelativeCommand.State.Pen.X);
        Assert.AreEqual(.125f, pointSetRelativeCommand.State.Pen.Y);
        Assert.AreEqual(0f, pointSetRelativeCommand.State.Pen.Z);
    }
}
