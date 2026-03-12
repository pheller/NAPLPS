// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Commands;

[TestClass]
public class ArcSetFilledCommandTests
{
    [TestMethod]
    public void Default()
    {
        var arcSetFilledCommand = new ArcSetFilledCommand(new(), 0xAF, new NaplpsOperands([]));

        Assert.IsFalse(arcSetFilledCommand.IsValid);

        Assert.IsTrue(arcSetFilledCommand.ShouldFill);
    }

    /// <summary>Based on https://archive.org/details/byte-magazine-1983-03/page/n160/mode/1up?view=theater</summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page159()
    {
        var arcSetFilledCommand = new ArcSetFilledCommand(new(), 0xAF, new NaplpsOperands([
            0x41, 0x77, 0x50,
            0x47, 0x47, 0x64,
            0x47, 0x47, 0x54,
        ]));

        Assert.IsTrue(arcSetFilledCommand.IsValid);

        Assert.IsTrue(arcSetFilledCommand.ShouldFill);

        // StartPoint = Vertex[0]
        Assert.AreEqual(.1953125f, arcSetFilledCommand.StartPoint.X);
        Assert.AreEqual(.46875f, arcSetFilledCommand.StartPoint.Y);
        Assert.AreEqual(0f, arcSetFilledCommand.StartPoint.Z);

        // IntermediatePointDisplacement = StartPoint + Vertex[1] (accumulated position)
        Assert.AreEqual(.2109375f, arcSetFilledCommand.IntermediatePointDisplacement.X);
        Assert.AreEqual(.453125f, arcSetFilledCommand.IntermediatePointDisplacement.Y);
        Assert.AreEqual(0f, arcSetFilledCommand.IntermediatePointDisplacement.Z);

        // EndPointDisplacement = IntermediatePointDisplacement + Vertex[2] (accumulated position)
        Assert.AreEqual(.21875f, arcSetFilledCommand.EndPointDisplacement.X);
        Assert.AreEqual(.4375f, arcSetFilledCommand.EndPointDisplacement.Y);
        Assert.AreEqual(0f, arcSetFilledCommand.EndPointDisplacement.Z);
    }
}
