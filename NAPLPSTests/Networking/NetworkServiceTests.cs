// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Net.Sockets;
using NAPLPS.Networking;

namespace NAPLPSTests.Networking;

/// <summary>
/// Smoke tests for the TCP NAPLPS bridge. These bind to ephemeral ports on the loopback
/// interface so they're safe to run in CI. Each test cleans up its listener in a finally.
/// </summary>
[TestClass]
public class NetworkServiceTests
{
    private static int FindFreePort()
    {
        // Bind to port 0 to get OS-assigned free port, then release.
        var l = new TcpListener(System.Net.IPAddress.Loopback, 0);
        l.Start();
        int port = ((System.Net.IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;
    }

    [TestMethod]
    public async Task SendAsync_DeliversBytesToReceivedBuffer()
    {
        int port = FindFreePort();
        using var svc = new NaplpsNetworkService();

        var receivedBytes = new TaskCompletionSource<byte[]>();
        svc.BytesReceived += b => receivedBytes.TrySetResult(b);

        svc.StartListening(port);

        try
        {
            var payload = new byte[] { 0x18, 0x1F, 0xC0, 0xC0, 0xA0, 0xC0, 0xC0 };
            await NaplpsNetworkService.SendAsync("127.0.0.1", port, payload);

            var got = await receivedBytes.Task.WaitAsync(System.TimeSpan.FromSeconds(5));
            CollectionAssert.AreEqual(payload, got, "Receiver got the same bytes the sender sent");

            // Buffer snapshot should match too.
            var snapshot = svc.SnapshotReceivedBuffer();
            CollectionAssert.AreEqual(payload, snapshot);
        }
        finally
        {
            svc.StopListening();
        }
    }

    [TestMethod]
    public void StartListening_Idempotent()
    {
        int port = FindFreePort();
        using var svc = new NaplpsNetworkService();

        svc.StartListening(port);
        Assert.IsTrue(svc.IsListening);

        // Second start on a different port should stop the first cleanly and bind anew.
        int otherPort = FindFreePort();
        svc.StartListening(otherPort);
        Assert.IsTrue(svc.IsListening);

        svc.StopListening();
        Assert.IsFalse(svc.IsListening);
    }

    [TestMethod]
    public void ClearReceivedBuffer_Empties()
    {
        using var svc = new NaplpsNetworkService();
        // No data sent; buffer is empty by default. Calling Clear should leave it empty.
        svc.ClearReceivedBuffer();
        Assert.AreEqual(0, svc.SnapshotReceivedBuffer().Length);
    }
}
