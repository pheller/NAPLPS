// tools/aot/rust/src/main.rs
//
// Read a .nap file, render it to PNG via the NAPLPS library, write the PNG.
// Usage:
//     naplps_demo <input.nap> <output.png> [width] [height]

use std::env;
use std::fs;
use std::process::ExitCode;

// FFI signatures matching tools/aot/include/naplps.h. `extern "C"` gives us the
// C ABI; the `unsafe` block wraps the raw-pointer API.
#[link(name = "NAPLPS", kind = "dylib")]
unsafe extern "C"
{
    fn naplps_render_png(
        nap_bytes: *const u8,
        nap_len: i32,
        width: i32,
        height: i32,
        out_buf: *mut u8,
        out_buf_len: i32,
    ) -> i32;

    fn naplps_command_count(nap_bytes: *const u8, nap_len: i32) -> i32;
    fn naplps_error_count(nap_bytes: *const u8, nap_len: i32) -> i32;
    fn naplps_version(out_buf: *mut u8, out_buf_len: i32) -> i32;
}

fn main() -> ExitCode
{
    let args: Vec<String> = env::args().collect();
    if args.len() < 3
    {
        eprintln!("usage: {} <input.nap> <output.png> [width] [height]", args[0]);
        return ExitCode::from(2);
    }

    let in_path = &args[1];
    let out_path = &args[2];
    let width: i32 = args.get(3).and_then(|s| s.parse().ok()).unwrap_or(1024);
    let height: i32 = args.get(4).and_then(|s| s.parse().ok()).unwrap_or(768);

    // Print version so we know the DLL loaded.
    let mut version = [0u8; 32];
    let vlen = unsafe { naplps_version(version.as_mut_ptr(), version.len() as i32) };
    if vlen < 0
    {
        eprintln!("naplps_version failed: {}", vlen);
        return ExitCode::FAILURE;
    }
    let vstr = std::str::from_utf8(&version[..vlen as usize]).unwrap_or("?");
    println!("NAPLPS library version: {}", vstr);

    let nap = match fs::read(in_path)
    {
        Ok(b) => b,
        Err(e) => { eprintln!("cannot read {}: {}", in_path, e); return ExitCode::FAILURE; }
    };
    println!("Loaded {} ({} bytes)", in_path, nap.len());

    let cmd_count = unsafe { naplps_command_count(nap.as_ptr(), nap.len() as i32) };
    let err_count = unsafe { naplps_error_count(nap.as_ptr(), nap.len() as i32) };
    println!("Parsed {} commands, {} errors", cmd_count, err_count);
    if cmd_count < 0
    {
        eprintln!("parse failed");
        return ExitCode::FAILURE;
    }

    // Query required buffer size.
    let required = unsafe { naplps_render_png(nap.as_ptr(), nap.len() as i32, width, height, std::ptr::null_mut(), 0) };
    if required < 0
    {
        eprintln!("render failed (query): {}", required);
        return ExitCode::FAILURE;
    }

    let mut png = vec![0u8; required as usize];
    let written = unsafe { naplps_render_png(nap.as_ptr(), nap.len() as i32, width, height, png.as_mut_ptr(), required) };
    if written < 0
    {
        eprintln!("render failed: {}", written);
        return ExitCode::FAILURE;
    }
    png.truncate(written as usize);

    if let Err(e) = fs::write(out_path, &png)
    {
        eprintln!("cannot write {}: {}", out_path, e);
        return ExitCode::FAILURE;
    }
    println!("Wrote {} ({} bytes, {}x{})", out_path, written, width, height);

    ExitCode::SUCCESS
}
