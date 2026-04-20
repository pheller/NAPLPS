// naplps_demo.js — Load the NAPLPS NativeAOT library via koffi FFI.
//
// Usage:
//     npm install                                       # once
//     node naplps_demo.js <input.nap> <output.png> [w] [h]
//
// The node.exe finds the DLL via the path passed to koffi.load. On Windows,
// koffi walks the PATH and the given directory; we pass the absolute path so
// no env config is needed.

const fs = require('node:fs');
const path = require('node:path');
const os = require('node:os');
const koffi = require('koffi');

function libraryPath()
{
    const publishDir = path.resolve(__dirname, '..', 'publish');
    switch (os.platform())
    {
        case 'win32':  return path.join(publishDir, 'NAPLPS.dll');
        case 'darwin': return path.join(publishDir, 'libNAPLPS.dylib');
        default:       return path.join(publishDir, 'libNAPLPS.so');
    }
}

function main()
{
    const args = process.argv.slice(2);
    if (args.length < 2)
    {
        console.error(`usage: ${process.argv[1]} <input.nap> <output.png> [width] [height]`);
        process.exit(2);
    }

    const [inPath, outPath] = args;
    const width  = args[2] ? parseInt(args[2], 10) : 1024;
    const height = args[3] ? parseInt(args[3], 10) : 768;

    const libPath = libraryPath();
    if (!fs.existsSync(libPath))
    {
        console.error(`NAPLPS library not found at ${libPath}. Run tools/aot/publish.ps1 first.`);
        process.exit(1);
    }

    const lib = koffi.load(libPath);

    // FFI signatures match tools/aot/include/naplps.h.
    const napBytes    = koffi.pointer(koffi.types.uint8);
    const outBuf      = koffi.pointer(koffi.types.uint8);
    const version     = lib.func('int32_t naplps_version(uint8_t* out_buf, int32_t out_buf_len)');
    const cmdCount    = lib.func('int32_t naplps_command_count(uint8_t* bytes, int32_t len)');
    const errCount    = lib.func('int32_t naplps_error_count(uint8_t* bytes, int32_t len)');
    const renderPng   = lib.func('int32_t naplps_render_png(uint8_t* bytes, int32_t len, int32_t w, int32_t h, uint8_t* out_buf, int32_t out_buf_len)');

    const versionBuf = Buffer.alloc(32);
    const vlen = version(versionBuf, 32);
    if (vlen < 0) { console.error(`naplps_version failed: ${vlen}`); process.exit(1); }
    console.log(`NAPLPS library version: ${versionBuf.slice(0, vlen).toString('ascii')}`);

    const nap = fs.readFileSync(inPath);
    console.log(`Loaded ${inPath} (${nap.length} bytes)`);

    const nCmds = cmdCount(nap, nap.length);
    const nErrs = errCount(nap, nap.length);
    console.log(`Parsed ${nCmds} commands, ${nErrs} errors`);
    if (nCmds < 0) { console.error('parse failed'); process.exit(1); }

    // Query required size, then render.
    const required = renderPng(nap, nap.length, width, height, null, 0);
    if (required < 0) { console.error(`render failed (query): ${required}`); process.exit(1); }

    const pngBuf = Buffer.alloc(required);
    const written = renderPng(nap, nap.length, width, height, pngBuf, required);
    if (written < 0) { console.error(`render failed: ${written}`); process.exit(1); }

    fs.writeFileSync(outPath, pngBuf.slice(0, written));
    console.log(`Wrote ${outPath} (${written} bytes, ${width}x${height})`);
}

main();
