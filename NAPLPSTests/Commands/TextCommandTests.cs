// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Numerics;


namespace NAPLPSTests.Commands;

[TestClass]
public class TextCommandTests
{
    [TestMethod]
    public void Defaults()
    {
        var textCommand = new TextCommand(new(), []);

        Assert.IsNotNull(textCommand);

        Assert.IsFalse(textCommand.IsValid);
    }

    /// <summary>https://archive.org/details/byte-magazine-1983-03/page/n163/mode/1up?view=theater</summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page162()
    {
        var textCommand = new TextCommand(new(), [0x4C]);

        Assert.IsNotNull(textCommand);

        Assert.IsTrue(textCommand.IsValid);

        Assert.AreEqual(TextRotation.Zero, textCommand.State.TextRotation);
        Assert.AreEqual(TextPath.Down, textCommand.State.TextPath);
        Assert.AreEqual(TextSpacing.One, textCommand.State.TextSpacing);

        Assert.AreEqual(TextInterrowSpacing.One, textCommand.State.TextInterrowSpacing);
        Assert.AreEqual(TextMoveAttributes.MoveTogether, textCommand.State.TextMoveAttributes);
        Assert.AreEqual(TextCursorStyle.Underscore, textCommand.State.TextCursorStyle);

        var defaults = new Vector2(1.0f / 40.0f, 5.0f / 128.0f);

        Assert.AreEqual(defaults.X, textCommand.State.TextFieldSize.X);
        Assert.AreEqual(defaults.Y, textCommand.State.TextFieldSize.Y);
    }

    /// <summary>https://archive.org/details/byte-magazine-1983-03/page/n164/mode/1up?view=theater</summary>
    [TestMethod]
    public void ByteMagazineMarch1983Page163()
    {
        const float x = .046875f;
        const float y = .078125f;

        var textCommand = new TextCommand(new(), [0x40, 0x40, 0x40, 0x4A, 0x64]);

        Assert.IsNotNull(textCommand);

        Assert.IsTrue(textCommand.IsValid);

        Assert.AreEqual(x, textCommand.Vertices[0].X);
        Assert.AreEqual(y, textCommand.Vertices[0].Y);

        Assert.AreEqual(TextRotation.Zero, textCommand.State.TextRotation);
        Assert.AreEqual(TextPath.Right, textCommand.State.TextPath);
        Assert.AreEqual(TextSpacing.One, textCommand.State.TextSpacing);

        Assert.AreEqual(TextInterrowSpacing.One, textCommand.State.TextInterrowSpacing);
        Assert.AreEqual(TextMoveAttributes.MoveTogether, textCommand.State.TextMoveAttributes);
        Assert.AreEqual(TextCursorStyle.Underscore, textCommand.State.TextCursorStyle);

        Assert.AreEqual(x, textCommand.State.TextFieldSize.X);
        Assert.AreEqual(y, textCommand.State.TextFieldSize.Y);
    }
}
