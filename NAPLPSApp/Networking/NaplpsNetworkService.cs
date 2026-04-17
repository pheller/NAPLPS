// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NAPLPSApp.Networking;

/// <summary>
/// Lightweight TCP server + client for streaming NAPLPS bytes between this editor and a
/// remote NAPLPS endpoint. NAPLPS was originally designed for videotex terminals over
/// serial / dial-up; this lets the editor act as either a transmitter (push the current
/// document to a viewer) or a receiver (accept a streamed scene and render incrementally).
/// </summary>
public class NaplpsNetworkService : IDisposable
{
    private TcpListener? _listener;
    private CancellationTokenSource? _listenerCts;
    private Task? _listenerTask;
    private readonly object _bufferLock = new();
    private readonly List<byte> _receiveBuffer = new();

    public bool IsListening => _listener != null;

    /// <summary>Fired (on a worker thread) whenever bytes arrive from a connected sender.
    /// Subscribers should marshal back to the UI thread before touching VM state.</summary>
    public event System.Action<byte[]>? BytesReceived;

    /// <summary>Fired when a client connects/disconnects. Argument is a human-readable status.</summary>
    public event System.Action<string>? StatusChanged;

    /// <summary>
    /// Begin listening for incoming NAPLPS streams on <paramref name="port"/>. Idempotent —
    /// stops any prior listener first. Each accepted client is read until close; bytes are
    /// raised via <see cref="BytesReceived"/> in chunks so the UI can render incrementally.
    /// </summary>
    public void StartListening(int port)
    {
        StopListening();

        _listener = new TcpListener(IPAddress.Any, port);
        _listenerCts = new CancellationTokenSource();
        _listener.Start();

        StatusChanged?.Invoke($"Listening on port {port}");
        _listenerTask = Task.Run(() => AcceptLoopAsync(_listenerCts.Token));
    }

    public void StopListening()
    {
        try
        {
            _listenerCts?.Cancel();
            _listener?.Stop();
        }
        catch { /* best-effort */ }

        _listener = null;
        _listenerCts = null;
        _listenerTask = null;
        StatusChanged?.Invoke("Stopped");
    }

    private async Task AcceptLoopAsync(CancellationToken ct)
    {
        if (_listener == null) { return; }

        while (!ct.IsCancellationRequested)
        {
            TcpClient client;

            try
            {
                client = await _listener.AcceptTcpClientAsync(ct);
            }
            catch (OperationCanceledException) { return; }
            catch (System.Exception ex) { StatusChanged?.Invoke($"Accept error: {ex.Message}"); return; }

            // Each client gets its own background read loop. Multiple concurrent senders
            // would interleave bytes — fine for stress test, weird for real use; the editor
            // shows the union of all streams as one rendered canvas.
            _ = Task.Run(() => ReadClientAsync(client, ct), ct);
        }
    }

    private async Task ReadClientAsync(TcpClient client, CancellationToken ct)
    {
        var endpoint = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
        StatusChanged?.Invoke($"Client connected: {endpoint}");

        try
        {
            using (client)
            using (var stream = client.GetStream())
            {
                var buf = new byte[4096];
                while (!ct.IsCancellationRequested)
                {
                    int n = await stream.ReadAsync(buf, 0, buf.Length, ct);
                    if (n <= 0) { break; }

                    var chunk = new byte[n];
                    System.Array.Copy(buf, chunk, n);

                    lock (_bufferLock)
                    {
                        _receiveBuffer.AddRange(chunk);
                    }

                    BytesReceived?.Invoke(chunk);
                }
            }
        }
        catch (System.Exception ex)
        {
            StatusChanged?.Invoke($"Read error: {ex.Message}");
        }

        StatusChanged?.Invoke($"Client disconnected: {endpoint}");
    }

    /// <summary>Snapshot the bytes received so far (for parsing into a NaplpsFormat).</summary>
    public byte[] SnapshotReceivedBuffer()
    {
        lock (_bufferLock)
        {
            return _receiveBuffer.ToArray();
        }
    }

    public void ClearReceivedBuffer()
    {
        lock (_bufferLock)
        {
            _receiveBuffer.Clear();
        }
    }

    /// <summary>
    /// Connect to <paramref name="host"/>:<paramref name="port"/> and push <paramref name="bytes"/>
    /// in a single send-and-close. Throws on connection failure; caller should wrap in try.
    /// </summary>
    public static async Task SendAsync(string host, int port, byte[] bytes, CancellationToken ct = default)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(host, port, ct);
        using var stream = client.GetStream();
        await stream.WriteAsync(bytes, 0, bytes.Length, ct);
        await stream.FlushAsync(ct);
    }

    public void Dispose()
    {
        StopListening();
    }
}
