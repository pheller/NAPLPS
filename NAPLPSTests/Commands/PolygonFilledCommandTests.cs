// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Commands;

[TestClass]
public class PolygonFilledCommandTests
{
    [TestMethod]
    public void Defaults()
    {
        var polygonFilledCommand = new PolygonFilledCommand(new(), 0xB5, new NaplpsOperands([]));

        Assert.IsNotNull(polygonFilledCommand);

        Assert.AreEqual(polygonFilledCommand.Vertices.Count, 0);
    }

    /// <summary>Based on https://archive.org/details/byte-magazine-1983-03/page/n157/mode/1up?view=theater</summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page156()
    {
        var polygonFilledCommand = new PolygonFilledCommand(new(), 0xB5, new NaplpsOperands([
            0x40, 0x61, 0x47,
            0x47, 0x66, 0x41,
        ]));

        Assert.IsNotNull(polygonFilledCommand);

        Assert.AreEqual(polygonFilledCommand.Vertices.Count, 2);

        Assert.AreEqual(polygonFilledCommand.Vertices[0].X, .125f);
        Assert.AreEqual(polygonFilledCommand.Vertices[0].Y, .05859375f);
        Assert.AreEqual(polygonFilledCommand.Vertices[0].Z, 0);

        Assert.AreEqual(polygonFilledCommand.Vertices[1].X, .125f);
        Assert.AreEqual(polygonFilledCommand.Vertices[1].Y, -.05859375f); // for some reason, the magazine has a bad value? (-.0625f)?
        Assert.AreEqual(polygonFilledCommand.Vertices[1].Z, 0);
    }
}
