// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Commands;

[TestClass]
public class PointSetAbsoluteCommandTests
{
    [TestMethod]
    public void Defaults()
    {
        var pointSetAbsoluteCommand = new PointSetAbsoluteCommand(new(), 0xA4, new NaplpsOperands([]));

        Assert.IsNotNull(pointSetAbsoluteCommand);

        Assert.AreEqual(0, pointSetAbsoluteCommand.Vertices.Count);

        Assert.AreEqual(0f, pointSetAbsoluteCommand.State.Pen.X);
        Assert.AreEqual(0f, pointSetAbsoluteCommand.State.Pen.Y);
        Assert.AreEqual(0f, pointSetAbsoluteCommand.State.Pen.Z);
    }

    [TestMethod]
    public void FirstTest()
    {
        var pointSetAbsoluteCommand = new PointSetAbsoluteCommand(new(), 0xA4, new NaplpsOperands([0x49, 0x60, 0x40]));

        Assert.IsNotNull(pointSetAbsoluteCommand);

        Assert.AreEqual(0, pointSetAbsoluteCommand.Vertices.Count);

        Assert.AreEqual(.375f, pointSetAbsoluteCommand.State.Pen.X);
        Assert.AreEqual(.25f, pointSetAbsoluteCommand.State.Pen.Y);
        Assert.AreEqual(0f, pointSetAbsoluteCommand.State.Pen.Z);
    }

    [TestMethod]
    public void SecondTest()
    {
        var pointSetAbsoluteCommand = new PointSetAbsoluteCommand(new(), 0xA4, new NaplpsOperands([0x4A, 0x56, 0x68]));

        Assert.IsNotNull(pointSetAbsoluteCommand);

        Assert.AreEqual(0, pointSetAbsoluteCommand.Vertices.Count);

        Assert.AreEqual(85.0f / 256.0f, pointSetAbsoluteCommand.State.Pen.X);
        Assert.AreEqual(176.0f / 256.0f, pointSetAbsoluteCommand.State.Pen.Y);
        Assert.AreEqual(0f, pointSetAbsoluteCommand.State.Pen.Z);
    }

    /// <summary>Based on https://archive.org/details/byte-magazine-1983-03/page/n156/mode/1up?view=theater</summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page156()
    {
        var pointSetAbsoluteCommand = new PointSetAbsoluteCommand(new(), 0xA4, new NaplpsOperands([0x48, 0x57, 0x44]));

        Assert.IsNotNull(pointSetAbsoluteCommand);

        Assert.AreEqual(0, pointSetAbsoluteCommand.Vertices.Count);

        Assert.AreEqual(.3125f, pointSetAbsoluteCommand.State.Pen.X);
        Assert.AreEqual(.234375f, pointSetAbsoluteCommand.State.Pen.Y);
        Assert.AreEqual(0f, pointSetAbsoluteCommand.State.Pen.Z);
    }
}
