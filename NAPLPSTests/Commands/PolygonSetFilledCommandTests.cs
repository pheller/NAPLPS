using NAPLPS.Commands;

namespace NAPLPSTests.Commands;

[TestClass]
public class PolygonSetFilledCommandTests
{
    [TestMethod]
    public void Defaults()
    {
        var setPolygonFilledCommand = new PolygonSetFilledCommand(new(), []);

        Assert.IsNotNull(setPolygonFilledCommand);

        Assert.IsFalse(setPolygonFilledCommand.IsValid);

        Assert.AreEqual(setPolygonFilledCommand.Vertices.Count, 0);

        Assert.AreEqual(setPolygonFilledCommand.StartPoint.X, 0);
        Assert.AreEqual(setPolygonFilledCommand.StartPoint.Y, 0);
        Assert.AreEqual(setPolygonFilledCommand.StartPoint.Z, 0);
    }

    [TestMethod]
    public void ByteMagazineMarch1983Page154()
    {
        var setPolygonFilledCommand = new PolygonSetFilledCommand(new(), [
            0x49, 0x60, 0x40, // First Multi-Byte Value
            0x48, 0x60, 0x40, // 2nd Multi-Byte Value
            0x48, 0x42, 0x40, // ...
            0x46, 0x46, 0x40,
            0x60, 0x40, 0x40,
            0x40, 0x46, 0x47,
            0x40, 0x6A, 0x60]);

        Assert.IsNotNull(setPolygonFilledCommand);

        Assert.IsTrue(setPolygonFilledCommand.IsValid);

        Assert.AreEqual(setPolygonFilledCommand.StartPoint.X, .375);
        Assert.AreEqual(setPolygonFilledCommand.StartPoint.Y, .25);
        Assert.AreEqual(setPolygonFilledCommand.StartPoint.Z, 0);

        Assert.AreEqual(setPolygonFilledCommand.Vertices.Count, 6);

        List<Tuple<double, double>> validVerts =
        [
            new (.375, 0),
            new (.25, .0625),
            new (0, -.3125),
            new (-1, 0),
            new (0, .21484375),
            new (.171875, .0625),
        ];

        for (int i = 0; i < setPolygonFilledCommand.Vertices.Count; i++)
        {
            Assert.AreEqual(setPolygonFilledCommand.Vertices[i].X, validVerts[i].Item1);
            Assert.AreEqual(setPolygonFilledCommand.Vertices[i].Y, validVerts[i].Item2);
            Assert.AreEqual(setPolygonFilledCommand.Vertices[i].Z, 0);
        }
    }
}
