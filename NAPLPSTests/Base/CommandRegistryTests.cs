// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Base;

[TestClass]
public class CommandRegistryTests
{
    [TestMethod]
    public void Registry_FindsRectangleSetFilled()
    {
        var descriptor = CommandRegistry.GetByType(typeof(RectangleSetFilledCommand));

        Assert.IsNotNull(descriptor);
        Assert.AreEqual(CommandCategory.Geometric, descriptor.Category);
        Assert.AreEqual("rectSetFilled", descriptor.DslKeyword);
        CollectionAssert.Contains((System.Collections.ICollection)descriptor.DefaultOpcodes, (byte)0xB3);
    }

    [TestMethod]
    public void Registry_MapsPdiOpcodesToTypes()
    {
        Assert.AreEqual(typeof(ResetCommand), CommandRegistry.GetByOpcode(0xA0)?.CommandType);
        Assert.AreEqual(typeof(DomainCommand), CommandRegistry.GetByOpcode(0xA1)?.CommandType);
        Assert.AreEqual(typeof(TextCommand), CommandRegistry.GetByOpcode(0xA2)?.CommandType);
        Assert.AreEqual(typeof(PointSetAbsoluteCommand), CommandRegistry.GetByOpcode(0xA4)?.CommandType);
        Assert.AreEqual(typeof(LineAbsoluteCommand), CommandRegistry.GetByOpcode(0xA8)?.CommandType);
        Assert.AreEqual(typeof(RectangleFilledCommand), CommandRegistry.GetByOpcode(0xB1)?.CommandType);
        Assert.AreEqual(typeof(PolygonFilledCommand), CommandRegistry.GetByOpcode(0xB5)?.CommandType);
        Assert.AreEqual(typeof(SelectColorCommand), CommandRegistry.GetByOpcode(0xBE)?.CommandType);
    }

    [TestMethod]
    public void Registry_FindsByDslKeyword()
    {
        Assert.AreEqual(typeof(LineAbsoluteCommand), CommandRegistry.GetByKeyword("lineAbs")?.CommandType);
        Assert.AreEqual(typeof(PolygonFilledCommand), CommandRegistry.GetByKeyword("polyFilled")?.CommandType);
        Assert.AreEqual(typeof(ResetCommand), CommandRegistry.GetByKeyword("reset")?.CommandType);
    }

    [TestMethod]
    public void Registry_KeywordLookupIsCaseInsensitive()
    {
        Assert.IsNotNull(CommandRegistry.GetByKeyword("LINEABS"));
        Assert.IsNotNull(CommandRegistry.GetByKeyword("lineabs"));
        Assert.IsNotNull(CommandRegistry.GetByKeyword("LineAbs"));
    }

    [TestMethod]
    public void Registry_CategoryEnumerationHasGeometry()
    {
        var geometric = CommandRegistry.ByCategory(CommandCategory.Geometric).ToList();

        Assert.IsTrue(geometric.Count >= 15, $"Expected at least 15 geometric commands, got {geometric.Count}");
        Assert.IsTrue(geometric.Any(d => d.CommandType == typeof(LineAbsoluteCommand)));
        Assert.IsTrue(geometric.Any(d => d.CommandType == typeof(ArcFilledCommand)));
    }

    [TestMethod]
    public void Registry_NumericalDataCoversHighRange()
    {
        var descriptor = CommandRegistry.GetByOpcode(0xC0);
        Assert.IsNotNull(descriptor);
        Assert.AreEqual(typeof(NumericalDataCommand), descriptor.CommandType);

        descriptor = CommandRegistry.GetByOpcode(0xFF);
        Assert.IsNotNull(descriptor);
        Assert.AreEqual(typeof(NumericalDataCommand), descriptor.CommandType);
    }
}
