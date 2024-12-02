// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Commands;

[TestClass]
public class PointSetRelativeCommandTests
{
    [TestMethod]
    public void Defaults()
    {
        var pointSetRelativeCommand = new PointSetRelativeCommand(new(), []);

        Assert.IsNotNull(pointSetRelativeCommand);

        Assert.AreEqual(pointSetRelativeCommand.Vertices.Count, 0);

        //Assert.AreEqual(pointSetRelativeCommand.Point.X, 0);
        //Assert.AreEqual(pointSetRelativeCommand.Point.Y, 0);
        //Assert.AreEqual(pointSetRelativeCommand.Point.Z, 0);
    }

    /// <summary>Based on https://archive.org/details/byte-magazine-1983-03/page/n157/mode/1up?view=theater</summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page156()
    {
        var pointSetAbsoluteCommand = new PointSetRelativeCommand(new(), [0x78, 0x44, 0x60]);

        Assert.IsNotNull(pointSetAbsoluteCommand);

        Assert.AreEqual(pointSetAbsoluteCommand.Vertices.Count, 0);

        //Assert.AreEqual(pointSetAbsoluteCommand.Point.X, -.234375f);
        //Assert.AreEqual(pointSetAbsoluteCommand.Point.Y, .125f);
        //Assert.AreEqual(pointSetAbsoluteCommand.Point.Z, 0);
    }
}
