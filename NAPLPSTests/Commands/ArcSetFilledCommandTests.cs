// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Commands;

[TestClass]
public class ArcSetFilledCommandTests
{
    [TestMethod]
    public void Default()
    {
        //var arcSetFilledCommand = new ArcSetFilledCommand(new(), []);

        //Assert.AreEqual(false, arcSetFilledCommand.IsValid);

        //Assert.AreEqual(true, arcSetFilledCommand.ShouldFill);
    }

    /// <summary>Based on https://archive.org/details/byte-magazine-1983-03/page/n160/mode/1up?view=theater</summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page159()
    {
        //var arcSetFilledCommand = new ArcSetFilledCommand(new(), [
        //    0x41, 0x77, 0x50,
        //    0x47, 0x47, 0x64,
        //    0x47, 0x47, 0x54,
        //]);

        //Assert.AreEqual(true, arcSetFilledCommand.IsValid);

        //Assert.AreEqual(true, arcSetFilledCommand.ShouldFill);

        //Assert.AreEqual(.1953125f, arcSetFilledCommand.StartPoint.X);
        //Assert.AreEqual(.46875f, arcSetFilledCommand.StartPoint.Y);
        //Assert.AreEqual(0, arcSetFilledCommand.StartPoint.Z);

        //Assert.AreEqual(.015625f, arcSetFilledCommand.IntermediatePointDisplacement.X);
        //Assert.AreEqual(-.015625f, arcSetFilledCommand.IntermediatePointDisplacement.Y);
        //Assert.AreEqual(0, arcSetFilledCommand.IntermediatePointDisplacement.Z);

        //Assert.AreEqual(.0078125f, arcSetFilledCommand.EndPointDisplacement.X);
        //Assert.AreEqual(-.015625f, arcSetFilledCommand.EndPointDisplacement.Y);
        //Assert.AreEqual(0, arcSetFilledCommand.EndPointDisplacement.Z);
    }
}
