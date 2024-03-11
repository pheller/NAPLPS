namespace NAPLPSTests.Commands;

[TestClass]
public class RectangleFilledCommandTests
{
    [TestMethod]
    public void Defaults()
    {
        var rectangleFilledCommand = new RectangleFilledCommand(new(), []);

        Assert.IsNotNull(rectangleFilledCommand);

        Assert.AreEqual(rectangleFilledCommand.Vertices.Count, 0);
    }

    /// <summary>Based on https://archive.org/details/byte-magazine-1983-03/page/n157/mode/1up?view=theater</summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page156()
    {
        var rectangleFilledCommand = new RectangleFilledCommand(new(), [0x40, 0x7C, 0x40]);

        Assert.IsNotNull(rectangleFilledCommand);

        Assert.AreEqual(rectangleFilledCommand.Vertices.Count, 0);

        Assert.AreEqual(rectangleFilledCommand.Dimensions.X, .21875f);
        Assert.AreEqual(rectangleFilledCommand.Dimensions.Y, .125f);
        Assert.AreEqual(rectangleFilledCommand.Dimensions.Z, 0);
    }
}
