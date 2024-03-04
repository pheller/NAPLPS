// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS;
using NAPLPS.Commands;

namespace NAPLPSTests.Commands;

[TestClass]
public class DomainCommandTests
{
    [TestMethod]
    public void TestPel5()
    {
        var domainCommand = new DomainCommand(new(), [0x48, 0x40, 0x40, 0x6D]);

        Assert.IsNotNull(domainCommand);

        Assert.AreEqual(domainCommand.State.Dimensionality, 2);
        Assert.AreEqual(domainCommand.State.SingleByteValue, 1);
        Assert.AreEqual(domainCommand.State.MultiByteValue, 3);

        Assert.AreEqual(domainCommand.State.LogicalPel.X, 5);
        Assert.AreEqual(domainCommand.State.LogicalPel.Y, 5);
    }


    [TestMethod]
    public void TestPel3()
    {
        var domainCommand = new DomainCommand(new(), [0x48, 0x40, 0x40, 0x5B]);

        Assert.IsNotNull(domainCommand);

        Assert.AreEqual(domainCommand.State.Dimensionality, 2);
        Assert.AreEqual(domainCommand.State.SingleByteValue, 1);
        Assert.AreEqual(domainCommand.State.MultiByteValue, 3);

        Assert.AreEqual(domainCommand.State.LogicalPel.X, 3);
        Assert.AreEqual(domainCommand.State.LogicalPel.Y, 3);
    }

    [TestMethod]
    public void TestPel1()
    {
        var domainCommand = new DomainCommand(new(), [0x48, 0x40, 0x40, 0x49]);

        Assert.IsNotNull(domainCommand);

        Assert.AreEqual(domainCommand.State.Dimensionality, 2);
        Assert.AreEqual(domainCommand.State.SingleByteValue, 1);
        Assert.AreEqual(domainCommand.State.MultiByteValue, 3);

        Assert.AreEqual(domainCommand.State.LogicalPel.X, 1);
        Assert.AreEqual(domainCommand.State.LogicalPel.Y, 1);
    }

    [TestMethod]
    public void Defaults()
    {
        var domainCommand = new DomainCommand(new(), []);

        Assert.IsNotNull(domainCommand);

        Assert.AreEqual(domainCommand.State.Dimensionality, 2);
        Assert.AreEqual(domainCommand.State.SingleByteValue, 1);
        Assert.AreEqual(domainCommand.State.MultiByteValue, 3);

        Assert.AreEqual(domainCommand.State.LogicalPel.X, 1);
        Assert.AreEqual(domainCommand.State.LogicalPel.Y, 1);
    }
}
