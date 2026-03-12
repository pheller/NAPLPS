// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Commands;

[TestClass]
public class DomainCommandTests
{
    [TestMethod]
    public void Defaults()
    {
        var domainCommand = new DomainCommand(new(), 0xA1, new NaplpsOperands([]));

        Assert.IsNotNull(domainCommand);

        // Empty operands means constructor returns early — defaults remain
        Assert.AreEqual(2, domainCommand.State.Dimensionality);
        Assert.AreEqual(1, domainCommand.State.SingleByteValue);
        Assert.AreEqual(3, domainCommand.State.MultiByteValue);

        Assert.AreEqual(0f, domainCommand.State.LogicalPel.X);
        Assert.AreEqual(0f, domainCommand.State.LogicalPel.Y);
    }

    [TestMethod]
    public void TestPel5()
    {
        var domainCommand = new DomainCommand(new(), 0xA1, new NaplpsOperands([0x48, 0x40, 0x40, 0x6D]));

        Assert.IsNotNull(domainCommand);

        Assert.AreEqual(2, domainCommand.State.Dimensionality);
        Assert.AreEqual(1, domainCommand.State.SingleByteValue);
        Assert.AreEqual(3, domainCommand.State.MultiByteValue);

        // LogicalPel is stored as normalized fraction from ProcessVertices
        Assert.AreEqual(0.01953125f, domainCommand.State.LogicalPel.X);
        Assert.AreEqual(0.01953125f, domainCommand.State.LogicalPel.Y);
    }

    [TestMethod]
    public void TestPel3()
    {
        var domainCommand = new DomainCommand(new(), 0xA1, new NaplpsOperands([0x48, 0x40, 0x40, 0x5B]));

        Assert.IsNotNull(domainCommand);

        Assert.AreEqual(2, domainCommand.State.Dimensionality);
        Assert.AreEqual(1, domainCommand.State.SingleByteValue);
        Assert.AreEqual(3, domainCommand.State.MultiByteValue);

        Assert.AreEqual(0.01171875f, domainCommand.State.LogicalPel.X);
        Assert.AreEqual(0.01171875f, domainCommand.State.LogicalPel.Y);
    }

    [TestMethod]
    public void TestPel1()
    {
        var domainCommand = new DomainCommand(new(), 0xA1, new NaplpsOperands([0x48, 0x40, 0x40, 0x49]));

        Assert.IsNotNull(domainCommand);

        Assert.AreEqual(2, domainCommand.State.Dimensionality);
        Assert.AreEqual(1, domainCommand.State.SingleByteValue);
        Assert.AreEqual(3, domainCommand.State.MultiByteValue);

        Assert.AreEqual(0.00390625f, domainCommand.State.LogicalPel.X);
        Assert.AreEqual(0.00390625f, domainCommand.State.LogicalPel.Y);
    }
}
