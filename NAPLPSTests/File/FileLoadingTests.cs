// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.File;

[TestClass]
public class FileLoadingTests
{
    [TestMethod]
    public void LoadMaple()
    {
        var file = NaplpsFormat.FromFile("../../../../examples/maple.nap");

        Assert.IsNotNull(file);

        Assert.IsFalse(file.IsErrored);
        Assert.IsTrue(file.IsValid);

        Assert.IsTrue(file.Is7Bit);

        Assert.AreEqual(9, file.Commands.Count);
    }

    [TestMethod]
    public void LoadAutumn()
    {
        var file = NaplpsFormat.FromFile("../../../../examples/autumn.nap");

        Assert.IsNotNull(file);

        Assert.IsFalse(file.IsErrored);
        Assert.IsTrue(file.IsValid);

        Assert.IsTrue(file.Is7Bit);

        Assert.AreEqual(269, file.Commands.Count);

        // file.SavePNG(_defaultSize, $"{_outputPath}autumn");
    }
}
