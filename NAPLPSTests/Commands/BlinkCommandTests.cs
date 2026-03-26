// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSTests.Commands;

[TestClass]
public class BlinkCommandTests
{
    [TestMethod]
    public void BlinkProcess_TimeUnit_Is100ms()
    {
        var process = new BlinkProcess
        {
            OnInterval = 5,
            OffInterval = 5,
        };

        // Tick by 500ms (5 * 100ms) — should complete one on-interval
        bool changed = process.Tick(500);

        Assert.IsTrue(changed);
    }

    [TestMethod]
    public void BlinkProcess_StartsInOnState()
    {
        var process = new BlinkProcess
        {
            OnInterval = 10,
            OffInterval = 10,
        };

        Assert.IsTrue(process.IsOn);
    }

    [TestMethod]
    public void BlinkProcess_TransitionsToOff()
    {
        var process = new BlinkProcess
        {
            OnInterval = 1,
            OffInterval = 10,
        };

        // Tick past the on interval (100ms)
        process.Tick(150);

        Assert.IsFalse(process.IsOn);
    }

    [TestMethod]
    public void BlinkProcess_StartDelay()
    {
        var process = new BlinkProcess
        {
            OnInterval = 1,
            OffInterval = 1,
            StartDelay = 5,
        };

        // Tick 200ms — still within start delay (500ms)
        process.Tick(200);

        Assert.IsTrue(process.IsOn); // Should still be in initial state during delay
    }
}
