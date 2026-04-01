// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Commands;

[TestClass]
public class WaitCommandTests
{
    // ANSI X3.110: WAIT operand byte 1 must be 0x5C (fixed format 1011100 in bits 7-1).
    // Subsequent bytes encode wait interval in bits 6-1 (the 6 data bits), range 0-63,
    // in 1/10 second units. Same encoding as BLINK intervals (& 0x3F).

    [TestMethod]
    public void InvalidFixedByte_MarksInvalid()
    {
        var command = new WaitCommand(new(), 0xBD, new NaplpsOperands([0x50, 0x42]));

        Assert.IsFalse(command.IsValid);
    }

    [TestMethod]
    public void ValidFixedByte_MarksValid()
    {
        var command = new WaitCommand(new(), 0xBD, new NaplpsOperands([0x5C, 0x42]));

        Assert.IsTrue(command.IsValid);
    }

    [TestMethod]
    public void TooFewOperands_MarksInvalid()
    {
        var command = new WaitCommand(new(), 0xBD, new NaplpsOperands([0x5C]));

        Assert.IsFalse(command.IsValid);
    }

    [TestMethod]
    public void WaitTime_ExtractsLower6Bits()
    {
        // 0x4A = 0x40 | 0x0A → value = 0x0A = 10 → 1.0 second
        var command = new WaitCommand(new(), 0xBD, new NaplpsOperands([0x5C, 0x4A]));

        Assert.IsTrue(command.IsValid);
        Assert.AreEqual(10, command.WaitTime);
    }

    [TestMethod]
    public void WaitTime_ZeroInterval()
    {
        // 0x40 = 0x40 | 0x00 → value = 0
        // Per spec: "A wait interval of zero is anywhere between 0 and .1 seconds long"
        var command = new WaitCommand(new(), 0xBD, new NaplpsOperands([0x5C, 0x40]));

        Assert.IsTrue(command.IsValid);
        Assert.AreEqual(0, command.WaitTime);
    }

    [TestMethod]
    public void WaitTime_MaxSingleByte()
    {
        // 0x7F = 0x40 | 0x3F → value = 63 → 6.3 seconds
        var command = new WaitCommand(new(), 0xBD, new NaplpsOperands([0x5C, 0x7F]));

        Assert.IsTrue(command.IsValid);
        Assert.AreEqual(63, command.WaitTime);
    }

    [TestMethod]
    public void WaitTime_SmallValue()
    {
        // 0x44 = 0x40 | 0x04 → value = 4 → 0.4 seconds (typical eye blink)
        var command = new WaitCommand(new(), 0xBD, new NaplpsOperands([0x5C, 0x44]));

        Assert.IsTrue(command.IsValid);
        Assert.AreEqual(4, command.WaitTime);
    }

    [TestMethod]
    public void MultipleWaitBytes_AreSummed()
    {
        // Byte 2: 0x45 → 5
        // Byte 3: 0x43 → 3
        // Total: WaitTime=5, WaitTimes=[3], sum = 8 (0.8s)
        var command = new WaitCommand(new(), 0xBD, new NaplpsOperands([0x5C, 0x45, 0x43]));

        Assert.IsTrue(command.IsValid);
        Assert.AreEqual(5, command.WaitTime);
        Assert.AreEqual(1, command.WaitTimes.Count);
        Assert.AreEqual(3, command.WaitTimes[0]);

        int totalTenths = command.WaitTime + command.WaitTimes.Sum(w => (int)w);
        Assert.AreEqual(8, totalTenths);
    }

    [TestMethod]
    public void MultipleWaitBytes_ManyBytes()
    {
        // 4 wait bytes: 10 + 20 + 30 + 3 = 63 (6.3 seconds)
        // 0x40 | 10 = 0x4A, 0x40 | 20 = 0x54, 0x40 | 30 = 0x5E, 0x40 | 3 = 0x43
        var command = new WaitCommand(new(), 0xBD, new NaplpsOperands([0x5C, 0x4A, 0x54, 0x5E, 0x43]));

        Assert.IsTrue(command.IsValid);
        Assert.AreEqual(10, command.WaitTime);
        Assert.AreEqual(3, command.WaitTimes.Count);

        int totalTenths = command.WaitTime + command.WaitTimes.Sum(w => (int)w);
        Assert.AreEqual(63, totalTenths);
    }

    [TestMethod]
    public void WaitTime_MatchesBlinkEncoding()
    {
        // Verify WaitCommand uses same extraction as BlinkCommand (& 0x3F)
        // Both should give 15 for operand byte 0x4F
        var command = new WaitCommand(new(), 0xBD, new NaplpsOperands([0x5C, 0x4F]));

        Assert.AreEqual(15, command.WaitTime); // 0x4F & 0x3F = 15
    }
}
