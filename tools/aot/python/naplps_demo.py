#!/usr/bin/env python3
"""naplps_demo.py — Load the NAPLPS NativeAOT library via ctypes, render a
.nap file to PNG.

Usage:
    python naplps_demo.py <input.nap> <output.png> [width] [height]

Requires the AOT-published library at ../publish/NAPLPS.dll (Windows) or the
.so / .dylib equivalent on Linux/macOS. Run tools/aot/publish.ps1 first.
"""
import ctypes
import os
import platform
import sys
from pathlib import Path


def _library_path() -> Path:
    script_dir = Path(__file__).resolve().parent
    publish = script_dir.parent / "publish"
    system = platform.system()
    if system == "Windows":
        return publish / "NAPLPS.dll"
    if system == "Darwin":
        return publish / "libNAPLPS.dylib"
    return publish / "libNAPLPS.so"


def _load() -> ctypes.CDLL:
    lib_path = _library_path()
    if not lib_path.exists():
        sys.exit(f"NAPLPS library not found at {lib_path}. Run tools/aot/publish.ps1 first.")
    lib = ctypes.CDLL(str(lib_path))

    # naplps_version(out_buf, out_buf_len) -> int32
    lib.naplps_version.argtypes = [ctypes.c_char_p, ctypes.c_int32]
    lib.naplps_version.restype = ctypes.c_int32

    # naplps_command_count(nap_bytes, nap_len) -> int32
    lib.naplps_command_count.argtypes = [ctypes.c_char_p, ctypes.c_int32]
    lib.naplps_command_count.restype = ctypes.c_int32

    # naplps_error_count(nap_bytes, nap_len) -> int32
    lib.naplps_error_count.argtypes = [ctypes.c_char_p, ctypes.c_int32]
    lib.naplps_error_count.restype = ctypes.c_int32

    # naplps_render_png(nap_bytes, nap_len, w, h, out_buf, out_buf_len) -> int32
    lib.naplps_render_png.argtypes = [
        ctypes.c_char_p, ctypes.c_int32,
        ctypes.c_int32, ctypes.c_int32,
        ctypes.c_char_p, ctypes.c_int32,
    ]
    lib.naplps_render_png.restype = ctypes.c_int32

    return lib


def render_png(lib: ctypes.CDLL, nap: bytes, width: int, height: int) -> bytes:
    # First call with empty buffer to query required size.
    required = lib.naplps_render_png(nap, len(nap), width, height, None, 0)
    if required < 0:
        raise RuntimeError(f"render failed (query): {required}")

    buf = ctypes.create_string_buffer(required)
    written = lib.naplps_render_png(nap, len(nap), width, height, buf, required)
    if written < 0:
        raise RuntimeError(f"render failed: {written}")
    return buf.raw[:written]


def main() -> int:
    if len(sys.argv) < 3:
        print(f"usage: {sys.argv[0]} <input.nap> <output.png> [width] [height]", file=sys.stderr)
        return 2

    in_path = sys.argv[1]
    out_path = sys.argv[2]
    width = int(sys.argv[3]) if len(sys.argv) > 3 else 1024
    height = int(sys.argv[4]) if len(sys.argv) > 4 else 768

    lib = _load()

    version_buf = ctypes.create_string_buffer(32)
    vlen = lib.naplps_version(version_buf, 32)
    print(f"NAPLPS library version: {version_buf.value.decode('ascii')}")

    with open(in_path, "rb") as f:
        nap = f.read()
    print(f"Loaded {in_path} ({len(nap)} bytes)")

    cmd_count = lib.naplps_command_count(nap, len(nap))
    err_count = lib.naplps_error_count(nap, len(nap))
    print(f"Parsed {cmd_count} commands, {err_count} errors")
    if cmd_count < 0:
        print("parse failed", file=sys.stderr)
        return 1

    png = render_png(lib, nap, width, height)
    with open(out_path, "wb") as f:
        f.write(png)
    print(f"Wrote {out_path} ({len(png)} bytes, {width}x{height})")
    return 0


if __name__ == "__main__":
    sys.exit(main())
