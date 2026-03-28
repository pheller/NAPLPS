// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.File;

[TestClass]
public class FileCreationTests
{
    [TestMethod]
    public void CreateNewFile_HasValidState()
    {
        var nap = NaplpsFormat.New();

        Assert.IsNotNull(nap);
        Assert.IsNotNull(nap.State);
        Assert.IsNotNull(nap.Commands);
    }

    [TestMethod]
    public void CreateNewFile_HasCommands()
    {
        var nap = NaplpsFormat.New();

        Assert.IsTrue(nap.Commands.Count > 0);
    }
}
