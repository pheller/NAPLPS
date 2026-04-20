<?php
/**
 * tools/aot/php/naplps_demo.php
 *
 * Load the NAPLPS NativeAOT library via PHP's built-in FFI (PHP 7.4+), render
 * a .nap file to PNG.
 *
 * Usage:
 *     php naplps_demo.php <input.nap> <output.png> [width] [height]
 *
 * Requires ext-ffi enabled in php.ini. Ships enabled by default in standard
 * Windows PHP builds; on Linux/macOS you may need `apt install php8.1-ffi` or
 * similar and enable it in php.ini with `extension=ffi`.
 */

declare(strict_types=1);

function libraryPath(): string
{
    $dir = __DIR__ . DIRECTORY_SEPARATOR . '..' . DIRECTORY_SEPARATOR . 'publish';
    $os = PHP_OS_FAMILY;
    return match ($os)
    {
        'Windows' => $dir . DIRECTORY_SEPARATOR . 'NAPLPS.dll',
        'Darwin'  => $dir . DIRECTORY_SEPARATOR . 'libNAPLPS.dylib',
        default   => $dir . DIRECTORY_SEPARATOR . 'libNAPLPS.so',
    };
}

function main(array $argv): int
{
    if (count($argv) < 3)
    {
        fwrite(STDERR, "usage: {$argv[0]} <input.nap> <output.png> [width] [height]\n");
        return 2;
    }

    $inPath  = $argv[1];
    $outPath = $argv[2];
    $width   = isset($argv[3]) ? (int) $argv[3] : 1024;
    $height  = isset($argv[4]) ? (int) $argv[4] : 768;

    if (!extension_loaded('ffi'))
    {
        fwrite(STDERR, "ext-ffi not enabled. Add `extension=ffi` to php.ini.\n");
        return 1;
    }

    $libPath = libraryPath();
    if (!file_exists($libPath))
    {
        fwrite(STDERR, "NAPLPS library not found at {$libPath}. Run tools/aot/publish.ps1 first.\n");
        return 1;
    }

    // Function signatures mirror tools/aot/include/naplps.h. PHP's FFI::cdef
    // takes C prototypes directly.
    $ffi = FFI::cdef(<<<C
        int32_t naplps_version(uint8_t *out_buf, int32_t out_buf_len);
        int32_t naplps_command_count(const uint8_t *nap_bytes, int32_t nap_len);
        int32_t naplps_error_count(const uint8_t *nap_bytes, int32_t nap_len);
        int32_t naplps_render_png(const uint8_t *nap_bytes, int32_t nap_len,
                                  int32_t width, int32_t height,
                                  uint8_t *out_buf, int32_t out_buf_len);
    C, $libPath);

    $versionBuf = FFI::new('uint8_t[32]');
    $vlen = $ffi->naplps_version($versionBuf, 32);
    if ($vlen < 0) { fwrite(STDERR, "naplps_version failed: {$vlen}\n"); return 1; }
    $versionStr = FFI::string(FFI::addr($versionBuf[0]), $vlen);
    echo "NAPLPS library version: {$versionStr}\n";

    $nap = file_get_contents($inPath);
    if ($nap === false) { fwrite(STDERR, "cannot read {$inPath}\n"); return 1; }
    $napLen = strlen($nap);
    echo "Loaded {$inPath} ({$napLen} bytes)\n";

    // Copy PHP string bytes into an FFI-owned buffer so the native side gets a
    // stable pointer for the full length of the call.
    $napBuf = FFI::new("uint8_t[{$napLen}]");
    FFI::memcpy($napBuf, $nap, $napLen);

    $nCmds = $ffi->naplps_command_count($napBuf, $napLen);
    $nErrs = $ffi->naplps_error_count($napBuf, $napLen);
    echo "Parsed {$nCmds} commands, {$nErrs} errors\n";
    if ($nCmds < 0) { fwrite(STDERR, "parse failed\n"); return 1; }

    // Query required PNG size, then render into a new buffer.
    $required = $ffi->naplps_render_png($napBuf, $napLen, $width, $height, null, 0);
    if ($required < 0) { fwrite(STDERR, "render failed (query): {$required}\n"); return 1; }

    $pngBuf = FFI::new("uint8_t[{$required}]");
    $written = $ffi->naplps_render_png($napBuf, $napLen, $width, $height, $pngBuf, $required);
    if ($written < 0) { fwrite(STDERR, "render failed: {$written}\n"); return 1; }

    $png = FFI::string(FFI::addr($pngBuf[0]), $written);
    file_put_contents($outPath, $png);
    echo "Wrote {$outPath} ({$written} bytes, {$width}x{$height})\n";
    return 0;
}

exit(main($argv));
