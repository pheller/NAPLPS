using System.Numerics;

namespace NAPLPSTests.Commands;

[TestClass]
public class RectangleSetFilledCommandTests
{
    [TestMethod]
    public void Defaults()
    {
        var rectangleSetFilledCommand = new RectangleSetFilledCommand(new(), []);

        Assert.IsNotNull(rectangleSetFilledCommand);

        Assert.IsFalse(rectangleSetFilledCommand.IsValid);
    }

    [TestMethod]
    public void Zero()
    {
        var rectangleSetFilledCommand = new RectangleSetFilledCommand(new(), [0x40, 0x40, 0x40, 0x40, 0x40, 0x40]);

        Assert.IsNotNull(rectangleSetFilledCommand);

        Assert.AreEqual(2, rectangleSetFilledCommand.Vertices.Count);

        Assert.AreEqual(Vector3.Zero, rectangleSetFilledCommand.StartPoint);

        Assert.AreEqual(Vector3.Zero, rectangleSetFilledCommand.Dimensions);
    }

    [TestMethod]
    public void NegativeDimensions()
    {
        var state = new NaplpsState()
        {
            MultiByteValue = 4,
            SingleByteValue = 1
        };

        var rectangleSetFilledCommand = new RectangleSetFilledCommand(state, [0x4A, 0x65, 0x45, 0x45, 0x47, 0x45, 0x5B, 0x66]);

        Assert.IsNotNull(rectangleSetFilledCommand);

        Assert.AreEqual(2, rectangleSetFilledCommand.Vertices.Count);

        Assert.AreEqual(new Vector3(0.375f, 0.67822266f, 0), rectangleSetFilledCommand.StartPoint);

        Assert.AreEqual(new Vector3(0.013671875f, -0.07910156f, 0), rectangleSetFilledCommand.Dimensions);
    }
}
