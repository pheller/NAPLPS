// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS.Commands;

namespace NAPLPSTests.Commands;

[TestClass]
public class PointSetAbsoluteCommandTests
{
    [TestMethod]
    public void Defaults()
    {
        var pointSetAbsoluteCommand = new PointSetAbsoluteCommand(new(), []);

        Assert.IsNotNull(pointSetAbsoluteCommand);

        Assert.AreEqual(pointSetAbsoluteCommand.Vertices.Count, 0);

        Assert.AreEqual(pointSetAbsoluteCommand.Point.X, 0);
        Assert.AreEqual(pointSetAbsoluteCommand.Point.Y, 0);
        Assert.AreEqual(pointSetAbsoluteCommand.Point.Z, 0);
    }

    [TestMethod]
    public void FirstTest()
    {
        var pointSetAbsoluteCommand = new PointSetAbsoluteCommand(new(), [0x49, 0x60, 0x40]);

        Assert.IsNotNull(pointSetAbsoluteCommand);

        Assert.AreEqual(pointSetAbsoluteCommand.Vertices.Count, 0);


        Assert.AreEqual(pointSetAbsoluteCommand.Point.X, .375);
        Assert.AreEqual(pointSetAbsoluteCommand.Point.Y, .25);
        Assert.AreEqual(pointSetAbsoluteCommand.Point.Z, 0);
    }

    [TestMethod]
    public void SecondTest()
    {
        var pointSetAbsoluteCommand = new PointSetAbsoluteCommand(new(), [0x4A, 0x56, 0x68]);

        Assert.IsNotNull(pointSetAbsoluteCommand);

        Assert.AreEqual(pointSetAbsoluteCommand.Vertices.Count, 0);


        Assert.AreEqual(pointSetAbsoluteCommand.Point.X, 85.0f/256.0f);
        Assert.AreEqual(pointSetAbsoluteCommand.Point.Y, 176.0f/256.0f);
        Assert.AreEqual(pointSetAbsoluteCommand.Point.Z, 0);
    }
}
