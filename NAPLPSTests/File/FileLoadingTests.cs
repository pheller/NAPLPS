// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAPLPSTests.File;

[TestClass]
public class FileLoadingTests
{
    [TestMethod]
    public void LoadMaple()
    {
        var file = NaplpsFile.FromFile("../../../../examples/maple.nap");

        Assert.IsNotNull(file);

        Assert.IsFalse(file.IsErrored);
        Assert.IsTrue(file.IsValid);

        Assert.IsTrue(file.Is7Bit);

        Assert.AreEqual(10, file.Commands.Count);
    }

    [TestMethod]
    public void LoadMapleAndSave()
    {
        var file = NaplpsFile.FromFile("../../../../examples/maple.nap");

        Assert.IsNotNull(file);

        Assert.IsFalse(file.IsErrored);
        Assert.IsTrue(file.IsValid);

        Assert.IsTrue(file.Is7Bit);

        Assert.AreEqual(10, file.Commands.Count);

        file.SavePNG("maple");
    }

    [TestMethod]
    public void LoadAutumnAndSave()
    {
        var file = NaplpsFile.FromFile("../../../../examples/autumn.nap");

        Assert.IsNotNull(file);

        Assert.IsFalse(file.IsErrored);
        Assert.IsTrue(file.IsValid);

        Assert.IsTrue(file.Is7Bit);

        Assert.AreEqual(270, file.Commands.Count);

        file.SavePNG("autumn");
    }
}
