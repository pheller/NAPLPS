// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Commands;

[TestClass]
public class TextCommandTests
{
    [TestMethod]
    public void TextCommand_SetsTextPath()
    {
        // TEXT PDI with path=Down: byte 1 bit 3,2 = 11 → 0x4C
        var command = new TextCommand(new(), 0xA3, new NaplpsOperands([0x4C]));

        Assert.AreEqual(TextCommand.TextPath.Down, command.State.TextPath);
    }

    [TestMethod]
    public void TextCommand_DefaultSpacing()
    {
        var command = new TextCommand(new(), 0xA3, new NaplpsOperands([0x40]));

        Assert.AreEqual(TextCommand.TextSpacing.One, command.State.TextSpacing);
    }

    [TestMethod]
    public void TextCommand_SetsCharSize()
    {
        // TEXT PDI with operand bytes that include char size vertices
        // Byte 1: 0x40 = all defaults, plus additional multi-value bytes for size
        var state = new NaplpsState();
        var command = new TextCommand(state, 0xA3, new NaplpsOperands([0x40, 0x40, 0x40, 0x4A, 0x64]));

        // CharSize should have been updated from the vertex data
        Assert.IsTrue(state.CharSize.X > 0);
        Assert.IsTrue(state.CharSize.Y > 0);
    }
}
