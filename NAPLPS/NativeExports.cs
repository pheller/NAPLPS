// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Runtime.InteropServices;
using NAPLPS.Drawing;
using SixLabors.ImageSharp.PixelFormats;

namespace NAPLPS;

/// <summary>
/// C-callable entry points exported by the NativeAOT-published NAPLPS library. Consumers
/// (C / C++ / Rust / anything with a P/Invoke or DllImport equivalent) can link against
/// the native .dll / .so / .dylib and call these directly. See tools/aot/ for working
/// examples in each language.
///
/// Error codes (negative return values):
///   -1  Parse error or exception.
///   -2  Output buffer too small. Call again with a larger buffer.
///   -3  Invalid input (null pointer or non-positive length).
///
/// Thread safety: functions are stateless and safe to call from multiple threads. Each
/// call builds its own DrawContext; there is no shared state across calls.
/// </summary>
public static unsafe class NativeExports
{
    /// <summary>
    /// Render a NAPLPS byte stream to a PNG image and copy the PNG bytes into
    /// <paramref name="outBuf"/>. Returns the PNG byte count on success, or a negative
    /// error code. A typical consumer calls this twice: first with a zero-length buffer
    /// to get the required size, then with a buffer of that size to receive the PNG.
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "naplps_render_png")]
    public static int RenderPng(byte* napBytes, int napLen, int width, int height, byte* outBuf, int outBufLen)
    {
        if (napBytes == null || napLen <= 0 || width <= 0 || height <= 0) { return -3; }

        try
        {
            var inBytes = new byte[napLen];
            Marshal.Copy((nint)napBytes, inBytes, 0, napLen);

            var format = NaplpsFormat.FromBytes(inBytes);
            using var ctx = new DrawContext(format, new SixLabors.ImageSharp.Size(width, height));
            ctx.Render();
            using var ms = new MemoryStream();
            ctx.Image.SaveAsPng(ms);
            var png = ms.ToArray();

            // Query pattern: caller passes outBufLen=0 to ask for the required size.
            if (outBuf == null || outBufLen < png.Length)
            {
                return outBufLen == 0 ? png.Length : -2;
            }

            Marshal.Copy(png, 0, (nint)outBuf, png.Length);
            return png.Length;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// Return the count of NAPLPS commands parsed from a byte stream. Useful for sanity-
    /// checking that a file loaded correctly. Returns a negative error code on failure.
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "naplps_command_count")]
    public static int CommandCount(byte* napBytes, int napLen)
    {
        if (napBytes == null || napLen <= 0) { return -3; }

        try
        {
            var inBytes = new byte[napLen];
            Marshal.Copy((nint)napBytes, inBytes, 0, napLen);
            var format = NaplpsFormat.FromBytes(inBytes);
            return format.Commands.Count;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// Return the count of parse errors recorded during load. Zero means a clean parse.
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "naplps_error_count")]
    public static int ErrorCount(byte* napBytes, int napLen)
    {
        if (napBytes == null || napLen <= 0) { return -3; }

        try
        {
            var inBytes = new byte[napLen];
            Marshal.Copy((nint)napBytes, inBytes, 0, napLen);
            var format = NaplpsFormat.FromBytes(inBytes);
            return format.Errors.Count;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// Library version string, ASCII, null-terminated, written into <paramref name="outBuf"/>.
    /// Returns the written length excluding the terminator, or a negative error code.
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "naplps_version")]
    public static int Version(byte* outBuf, int outBufLen)
    {
        const string version = "0.11.0";
        var bytes = System.Text.Encoding.ASCII.GetBytes(version);
        if (outBuf == null || outBufLen < bytes.Length + 1) { return outBufLen == 0 ? bytes.Length + 1 : -2; }
        Marshal.Copy(bytes, 0, (nint)outBuf, bytes.Length);
        outBuf[bytes.Length] = 0;
        return bytes.Length;
    }
}
