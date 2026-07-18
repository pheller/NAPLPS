// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using System.Reflection;
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
    /// Like <see cref="RenderPng"/> but forces the Prodigy pipeline: the stream is parsed as Prodigy
    /// (canonical CLUT + device metrics) and rendered with 2-bit color guns, hard text, the MVDI
    /// vector font, authentic integer geometry, and the Prodigy display ratio - i.e. the tuned output
    /// the reference render scoreboard targets. For known-Prodigy corpora whose files lack the A1 C8
    /// domain marker and would otherwise detect as generic NAPLPS (<see cref="RenderPng"/> detects
    /// the system type from the stream header).
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "naplps_render_png_prodigy")]
    public static int RenderPngProdigy(byte* napBytes, int napLen, int width, int height, byte* outBuf, int outBufLen)
    {
        if (napBytes == null || napLen <= 0 || width <= 0 || height <= 0) { return -3; }

        try
        {
            var inBytes = new byte[napLen];
            Marshal.Copy((nint)napBytes, inBytes, 0, napLen);

            var format = NaplpsFormat.FromBytes(inBytes, NaplpsSystemType.Prodigy);
            using var ctx = new DrawContext(format, new SixLabors.ImageSharp.Size(width, height));
            // DrawContext already enables the Prodigy pipeline from SystemType == Prodigy; set the
            // authentic-geometry flag explicitly so the thumbnail matches the exporter exactly.
            ctx.AuthenticGeometry = true;
            ctx.Render();
            using var ms = new MemoryStream();
            ctx.Image.SaveAsPng(ms);
            var png = ms.ToArray();

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
    /// Returns the written length excluding the terminator; when outBufLen is 0, returns the
    /// required buffer size including the terminator; negative on error.
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "naplps_version")]
    public static int Version(byte* outBuf, int outBufLen)
    {
        // Single-sourced from the csproj InformationalVersion; the SDK may append "+<commit>",
        // which is stripped for a clean C-string.
        var version = (typeof(NativeExports).Assembly
            .GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "0.0.0").Split('+')[0];
        var bytes = System.Text.Encoding.ASCII.GetBytes(version);
        if (outBuf == null || outBufLen < bytes.Length + 1) { return outBufLen == 0 ? bytes.Length + 1 : -2; }
        Marshal.Copy(bytes, 0, (nint)outBuf, bytes.Length);
        outBuf[bytes.Length] = 0;
        return bytes.Length;
    }
}
