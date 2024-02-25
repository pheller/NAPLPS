// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS;
using NAPLPS.Commands;

namespace NAPLPSTests.Commands;

[TestClass]
public class DomainCommandTests
{
    [TestMethod]
    public void BaseTestPel5()
    {
        var domainCommand = new DomainCommand([0x48, 0x40, 0x40, 0x6D]);

        Assert.IsNotNull(domainCommand);

        Assert.AreEqual(domainCommand.Dimensionality, 2);
        Assert.AreEqual(domainCommand.SingleByteValue, 1);
        Assert.AreEqual(domainCommand.MultiByteValue, 3);

        Assert.AreEqual(domainCommand.LogicalPel.X, 5);
        Assert.AreEqual(domainCommand.LogicalPel.Y, 5);
    }


    [TestMethod]
    public void BaseTestPel3()
    {
        var domainCommand = new DomainCommand([0x48, 0x40, 0x40, 0x5B]);

        Assert.IsNotNull(domainCommand);

        Assert.AreEqual(domainCommand.Dimensionality, 2);
        Assert.AreEqual(domainCommand.SingleByteValue, 1);
        Assert.AreEqual(domainCommand.MultiByteValue, 3);

        Assert.AreEqual(domainCommand.LogicalPel.X, 3);
        Assert.AreEqual(domainCommand.LogicalPel.Y, 3);
    }

    [TestMethod]
    public void BaseTestPel1()
    {
        var domainCommand = new DomainCommand([0x48, 0x40, 0x40, 0x49]);

        Assert.IsNotNull(domainCommand);

        Assert.AreEqual(domainCommand.Dimensionality, 2);
        Assert.AreEqual(domainCommand.SingleByteValue, 1);
        Assert.AreEqual(domainCommand.MultiByteValue, 3);

        Assert.AreEqual(domainCommand.LogicalPel.X, 1);
        Assert.AreEqual(domainCommand.LogicalPel.Y, 1);
    }

    [TestMethod]
    public void Defaults()
    {
        var domainCommand = new DomainCommand([]);

        Assert.IsNotNull(domainCommand);

        Assert.AreEqual(domainCommand.Dimensionality, 2);
        Assert.AreEqual(domainCommand.SingleByteValue, 1);
        Assert.AreEqual(domainCommand.MultiByteValue, 3);

        Assert.AreEqual(domainCommand.LogicalPel.X, 1);
        Assert.AreEqual(domainCommand.LogicalPel.Y, 1);
    }
}
