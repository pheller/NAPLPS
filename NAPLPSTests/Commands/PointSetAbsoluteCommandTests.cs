// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Commands;

[TestClass]
public class PointSetAbsoluteCommandTests
{
    [TestMethod]
    public void Defaults()
    {
        //var pointSetAbsoluteCommand = new PointSetAbsoluteCommand(new(), []);

        //Assert.IsNotNull(pointSetAbsoluteCommand);

        //Assert.AreEqual(pointSetAbsoluteCommand.Vertices.Count, 0);

        //Assert.AreEqual(pointSetAbsoluteCommand.Point.X, 0);
        //Assert.AreEqual(pointSetAbsoluteCommand.Point.Y, 0);
        //Assert.AreEqual(pointSetAbsoluteCommand.Point.Z, 0);
    }

    [TestMethod]
    public void FirstTest()
    {
        //var pointSetAbsoluteCommand = new PointSetAbsoluteCommand(new(), [0x49, 0x60, 0x40]);

        //Assert.IsNotNull(pointSetAbsoluteCommand);

        //Assert.AreEqual(pointSetAbsoluteCommand.Vertices.Count, 0);

        //Assert.AreEqual(pointSetAbsoluteCommand.Point.X, .375);
        //Assert.AreEqual(pointSetAbsoluteCommand.Point.Y, .25);
        //Assert.AreEqual(pointSetAbsoluteCommand.Point.Z, 0);
    }

    [TestMethod]
    public void SecondTest()
    {
        //var pointSetAbsoluteCommand = new PointSetAbsoluteCommand(new(), [0x4A, 0x56, 0x68]);

        //Assert.IsNotNull(pointSetAbsoluteCommand);

        //Assert.AreEqual(pointSetAbsoluteCommand.Vertices.Count, 0);

        //Assert.AreEqual(pointSetAbsoluteCommand.Point.X, 85.0f / 256.0f);
        //Assert.AreEqual(pointSetAbsoluteCommand.Point.Y, 176.0f / 256.0f);
        //Assert.AreEqual(pointSetAbsoluteCommand.Point.Z, 0);
    }

    /// <summary>Based on https://archive.org/details/byte-magazine-1983-03/page/n156/mode/1up?view=theater</summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page156()
    {
        //var pointSetAbsoluteCommand = new PointSetAbsoluteCommand(new(), [0x48, 0x57, 0x44]);

        //Assert.IsNotNull(pointSetAbsoluteCommand);

        //Assert.AreEqual(pointSetAbsoluteCommand.Vertices.Count, 0);

        //Assert.AreEqual(pointSetAbsoluteCommand.Point.X, .3125f);
        //Assert.AreEqual(pointSetAbsoluteCommand.Point.Y, .234375f);
        //Assert.AreEqual(pointSetAbsoluteCommand.Point.Z, 0);
    }
}
