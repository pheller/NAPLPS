// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Commands;

[TestClass]
public class ArcFilledCommandTests
{
    [TestMethod]
    public void Default()
    {
        var arcFilledCommand = new ArcFilledCommand(new(), 0xAD, new NaplpsOperands([]));

        Assert.IsFalse(arcFilledCommand.IsValid);

        Assert.IsTrue(arcFilledCommand.ShouldFill);
    }

    /// <summary>Based on https://archive.org/details/byte-magazine-1983-03/page/n160/mode/1up?view=theater</summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page159()
    {
        var arcFilledCommand = new ArcFilledCommand(new(), 0xAD, new NaplpsOperands([
            0x40, 0x40, 0x54,
            0x40, 0x40, 0x76,
        ]));

        Assert.IsTrue(arcFilledCommand.IsValid);

        Assert.IsTrue(arcFilledCommand.ShouldFill);

        // StartPoint = current pen (0,0,0)
        Assert.AreEqual(0f, arcFilledCommand.StartPoint.X);
        Assert.AreEqual(0f, arcFilledCommand.StartPoint.Y);
        Assert.AreEqual(0f, arcFilledCommand.StartPoint.Z);

        // IntermediatePointDisplacement = StartPoint + Vertex[0]
        Assert.AreEqual(.0078125f, arcFilledCommand.IntermediatePointDisplacement.X);
        Assert.AreEqual(.015625f, arcFilledCommand.IntermediatePointDisplacement.Y);
        Assert.AreEqual(0f, arcFilledCommand.IntermediatePointDisplacement.Z);

        // EndPointDisplacement = IntermediatePointDisplacement + Vertex[1]
        Assert.AreEqual(.03125f, arcFilledCommand.EndPointDisplacement.X);
        Assert.AreEqual(.0390625f, arcFilledCommand.EndPointDisplacement.Y);
        Assert.AreEqual(0f, arcFilledCommand.EndPointDisplacement.Z);
    }
}
