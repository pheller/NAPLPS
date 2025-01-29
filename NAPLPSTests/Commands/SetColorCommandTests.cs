// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Commands;

[TestClass]
public class SetColorCommandTests
{
    [TestMethod]
    public void Defaults()
    {
        //var command = new SetColorCommand(new(), []);

        //Assert.IsNotNull(command);

        //Assert.IsTrue(command.State.IsTransparent);
    }

    [TestMethod]
    public void TestConversionWhiteTo6Bit()
    {
        //var color6Bit = Color.White.To6BitRGB();

        //Assert.IsNotNull(color6Bit);

        //Assert.AreEqual(63, color6Bit.green6Bit);
        //Assert.AreEqual(63, color6Bit.red6Bit);
        //Assert.AreEqual(63, color6Bit.blue6Bit);
    }

    [TestMethod]
    public void TestConversion6BitToWhite()
    {
        //var color = Color.Empty.From6BitRGB(63, 63, 63);

        //Assert.IsNotNull(color);

        //Assert.AreEqual(color.G, Color.White.G);
        //Assert.AreEqual(color.R, Color.White.R);
        //Assert.AreEqual(color.B, Color.White.B);
    }

    [TestMethod]
    public void TestGreen()
    {
        //var command = new SetColorCommand(new(), [0x64, 0x64, 0x64]);

        //Assert.IsNotNull(command);

        //Assert.AreEqual(63, command.State.Foreground.Green);
        //Assert.AreEqual(0, command.State.Foreground.Red);
        //Assert.AreEqual(0, command.State.Foreground.Blue);
    }

    [TestMethod]
    public void TestRed()
    {
        //var command = new SetColorCommand(new(), [0x52, 0x52, 0x52]);

        //Assert.IsNotNull(command);

        //Assert.AreEqual(0, command.State.Foreground.Green);
        //Assert.AreEqual(63, command.State.Foreground.Red);
        //Assert.AreEqual(0, command.State.Foreground.Blue);
    }

    [TestMethod]
    public void TestBlue()
    {
        //var command = new SetColorCommand(new(), [0x49, 0x49, 0x49]);

        //Assert.IsNotNull(command);

        //Assert.AreEqual(0, command.State.Foreground.Green);
        //Assert.AreEqual(0, command.State.Foreground.Red);
        //Assert.AreEqual(63, command.State.Foreground.Blue);
    }

    [TestMethod]
    public void TestYellow()
    {
        //var command = new SetColorCommand(new(), [0x76, 0x76, 0x76]);

        //Assert.IsNotNull(command);

        //Assert.AreEqual(63, command.State.Foreground.Green);
        //Assert.AreEqual(63, command.State.Foreground.Red);
        //Assert.AreEqual(0, command.State.Foreground.Blue);
    }

    [TestMethod]
    public void TestBrown()
    {
        //var command = new SetColorCommand(new(), [0x50, 0x60, 0x56]);

        //Assert.IsNotNull(command);

        //Assert.AreEqual(9, command.State.Foreground.Green);
        //Assert.AreEqual(35, command.State.Foreground.Red);
        //Assert.AreEqual(0, command.State.Foreground.Blue);
    }

    [TestMethod]
    public void TestWhite()
    {
        //var command = new SetColorCommand(new(), [0x7F, 0x7F, 0x7F]);

        //Assert.IsNotNull(command);

        //Assert.AreEqual(63, command.State.Foreground.Green);
        //Assert.AreEqual(63, command.State.Foreground.Red);
        //Assert.AreEqual(63, command.State.Foreground.Blue);
    }

    [TestMethod]
    public void TestBlack()
    {
        //var command = new SetColorCommand(new(), [0x40, 0x40, 0x40]);

        //Assert.IsNotNull(command);

        //Assert.AreEqual(0, command.State.Foreground.Green);
        //Assert.AreEqual(0, command.State.Foreground.Red);
        //Assert.AreEqual(0, command.State.Foreground.Blue);
    }

    /// <summary>Based on https://archive.org/details/byte-magazine-1983-03/page/n154/mode/1up?view=theater</summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page154Blue()
    {
        //var command = new SetColorCommand(new(), [0x49]);

        //Assert.IsNotNull(command);
        //Assert.AreEqual(0, command.State.Foreground.Green);
        //Assert.AreEqual(0, command.State.Foreground.Red);
        //Assert.AreEqual(63, command.State.Foreground.Blue);
    }

    /// <summary>Based on https://archive.org/details/byte-magazine-1983-03/page/n154/mode/1up?view=theater</summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page154Green()
    {
        //var command = new SetColorCommand(new(), [0x64]);

        //Assert.IsNotNull(command);
        //Assert.AreEqual(63, command.State.Foreground.Green);
        //Assert.AreEqual(0, command.State.Foreground.Red);
        //Assert.AreEqual(0, command.State.Foreground.Blue);
    }

    /// <summary>Based on https://archive.org/details/byte-magazine-1983-03/page/n156/mode/1up?view=theater</summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page156Red()
    {
        //var command = new SetColorCommand(new(), [0x52]);

        //Assert.IsNotNull(command);
        //Assert.AreEqual(0, command.State.Foreground.Green);
        //Assert.AreEqual(63, command.State.Foreground.Red);
        //Assert.AreEqual(0, command.State.Foreground.Blue);
    }

    /// <summary>Based on https://archive.org/details/byte-magazine-1983-03/page/n156/mode/1up?view=theater</summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page156Cyan()
    {
        //var command = new SetColorCommand(new(), [0x6D]);

        //Assert.IsNotNull(command);

        //Assert.AreEqual(63, command.State.Foreground.Green);
        //Assert.AreEqual(0, command.State.Foreground.Red);
        //Assert.AreEqual(63, command.State.Foreground.Blue);
    }

    /// <summary>Based on https://archive.org/details/byte-magazine-1983-03/page/n162/mode/1up?view=theater</summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page162White()
    {
        //var command = new SetColorCommand(new(), [0x7F]);

        //Assert.IsNotNull(command);

        //Assert.AreEqual(63, command.State.Foreground.Green);
        //Assert.AreEqual(63, command.State.Foreground.Red);
        //Assert.AreEqual(63, command.State.Foreground.Blue);
    }

    /// <summary>Based on https://archive.org/details/byte-magazine-1983-03/page/n163/mode/1up?view=theater</summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page163Transparent()
    {
        //var command = new SetColorCommand(new(), [0x40]);

        //Assert.IsNotNull(command);

        //Assert.AreEqual(0, command.State.Foreground.Green);
        //Assert.AreEqual(0, command.State.Foreground.Red);
        //Assert.AreEqual(0, command.State.Foreground.Blue);
    }

    /// <summary>Based on https://archive.org/details/byte-magazine-1983-03/page/n164/mode/1up?view=theater</summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page164Yellow()
    {
        //var command = new SetColorCommand(new(), [0x76]);

        //Assert.IsNotNull(command);

        //Assert.AreEqual(63, command.State.Foreground.Green);
        //Assert.AreEqual(63, command.State.Foreground.Red);
        //Assert.AreEqual(0, command.State.Foreground.Blue);
    }
}