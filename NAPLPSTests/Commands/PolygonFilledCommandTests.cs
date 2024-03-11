namespace NAPLPSTests.Commands;

[TestClass]
public class PolygonFilledCommandTests
{
    [TestMethod]
    public void Defaults()
    {
        var polygonFilledCommand = new PolygonFilledCommand(new(), []);

        Assert.IsNotNull(polygonFilledCommand);

        Assert.AreEqual(polygonFilledCommand.Vertices.Count, 0);
    }

    /// <summary>Based on https://archive.org/details/byte-magazine-1983-03/page/n157/mode/1up?view=theater</summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page156()
    {
        var pointSetAbsoluteCommand = new PolygonFilledCommand(new(), [
            0x40, 0x61, 0x47,
            0x47, 0x66, 0x41,
        ]);

        Assert.IsNotNull(pointSetAbsoluteCommand);

        Assert.AreEqual(pointSetAbsoluteCommand.Vertices.Count, 2);

        //Assert.AreEqual(pointSetAbsoluteCommand.Point.X, -.234375f);
        //Assert.AreEqual(pointSetAbsoluteCommand.Point.Y, .125f);
        //Assert.AreEqual(pointSetAbsoluteCommand.Point.Z, 0);
    }
}
