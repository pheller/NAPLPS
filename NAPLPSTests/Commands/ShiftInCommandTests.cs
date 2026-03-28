// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Commands;

[TestClass]
public class ShiftInCommandTests
{
    [TestMethod]
    public void ShiftIn_SwitchesGLToPrimaryCharSet()
    {
        var state = new NaplpsState();

        // ShiftIn (0x0F) should switch GL back to the primary character set
        // After ShiftOut puts PDI into GL, ShiftIn restores it
        state.DoShiftOut(); // First shift out to PDI

        state.DoShiftIn(); // Now shift back

        // Verify GL is back to primary character set (ASCII)
        // The InUseTable at position 0x41 ('A') should be an AsciiCharCommand
        var entry = state.InUseTable[0x41];
        Assert.IsNotNull(entry);
        Assert.AreEqual(typeof(AsciiCharCommand), entry.CommandType);
    }
}
