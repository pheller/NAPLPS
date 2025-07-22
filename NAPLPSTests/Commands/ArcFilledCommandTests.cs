// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Commands;

[TestClass]
public class ArcFilledCommandTests
{
    [TestMethod]
    public void Default()
    {
        //var arcFilledCommand = new ArcFilledCommand(new(), 0x []);

        //Assert.AreEqual(false, arcFilledCommand.IsValid);

        //Assert.AreEqual(true, arcFilledCommand.ShouldFill);
    }

    /// <summary>Based on https://archive.org/details/byte-magazine-1983-03/page/n160/mode/1up?view=theater</summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page159()
    {
        //var arcFilledCommand = new ArcFilledCommand(new(), [
        //    0x40, 0x40, 0x54,
        //    0x40, 0x40, 0x76,
        //]);

        //Assert.AreEqual(true, arcFilledCommand.IsValid);

        //Assert.AreEqual(true, arcFilledCommand.ShouldFill);

        //Assert.AreEqual(0, arcFilledCommand.StartPoint.X);
        //Assert.AreEqual(0, arcFilledCommand.StartPoint.Y);
        //Assert.AreEqual(0, arcFilledCommand.StartPoint.Z);

        //Assert.AreEqual(.0078125f, arcFilledCommand.IntermediatePointDisplacement.X);
        //Assert.AreEqual(.015625f, arcFilledCommand.IntermediatePointDisplacement.Y);
        //Assert.AreEqual(0, arcFilledCommand.IntermediatePointDisplacement.Z);

        //Assert.AreEqual(.0234375f, arcFilledCommand.EndPointDisplacement.X);
        //Assert.AreEqual(.0234375f, arcFilledCommand.EndPointDisplacement.Y);
        //Assert.AreEqual(0, arcFilledCommand.EndPointDisplacement.Z);
    }
}
