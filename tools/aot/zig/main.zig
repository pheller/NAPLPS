// tools/aot/zig/main.zig
//
// Render Examples/telidraw/hello.nap to hello.png via the NAPLPS NativeAOT
// library. Zig's FFI story is just `extern fn` declarations. File I/O goes
// through libc fopen/fread/fwrite via @cImport because Zig 0.16's std.fs API
// is still in flux; libc is stable and already linked (build.zig turns on
// link_libc).
//
// Build with `zig build`, run from the zig/ directory.

const std = @import("std");

const c = @cImport({
    @cInclude("stdio.h");
    @cInclude("stdlib.h");
    @cInclude("string.h");
});

extern fn naplps_version(out_buf: [*]u8, out_buf_len: i32) i32;
extern fn naplps_command_count(nap_bytes: [*]const u8, nap_len: i32) i32;
extern fn naplps_error_count(nap_bytes: [*]const u8, nap_len: i32) i32;
extern fn naplps_render_png(
    nap_bytes: [*]const u8,
    nap_len: i32,
    width: i32,
    height: i32,
    out_buf: ?[*]u8,
    out_buf_len: i32,
) i32;

const in_path = "../../../Examples/telidraw/hello.nap";
const out_path = "hello.png";
const width: i32 = 1024;
const height: i32 = 768;

fn readFile(path: [*:0]const u8, len: *usize) ?[*]u8 {
    const f = c.fopen(path, "rb") orelse return null;
    defer _ = c.fclose(f);
    _ = c.fseek(f, 0, c.SEEK_END);
    const n: usize = @intCast(c.ftell(f));
    c.rewind(f);
    const buf = @as([*]u8, @ptrCast(c.malloc(n) orelse return null));
    _ = c.fread(buf, 1, n, f);
    len.* = n;
    return buf;
}

fn writeFile(path: [*:0]const u8, buf: [*]const u8, n: usize) bool {
    const f = c.fopen(path, "wb") orelse return false;
    defer _ = c.fclose(f);
    return c.fwrite(buf, 1, n, f) == n;
}

pub fn main() !u8 {
    var version_buf: [32]u8 = undefined;
    const vlen = naplps_version(&version_buf, version_buf.len);
    if (vlen < 0) {
        std.debug.print("naplps_version failed: {d}\n", .{vlen});
        return 1;
    }
    std.debug.print("NAPLPS library version: {s}\n", .{version_buf[0..@intCast(vlen)]});

    var nap_len: usize = 0;
    const nap = readFile(in_path, &nap_len) orelse {
        std.debug.print("cannot read {s}\n", .{in_path});
        return 1;
    };
    defer c.free(nap);
    std.debug.print("Loaded {s} ({d} bytes)\n", .{ in_path, nap_len });

    const nap_len_i: i32 = @intCast(nap_len);

    const n_cmds = naplps_command_count(nap, nap_len_i);
    const n_errs = naplps_error_count(nap, nap_len_i);
    std.debug.print("Parsed {d} commands, {d} errors\n", .{ n_cmds, n_errs });
    if (n_cmds < 0) return 1;

    const required = naplps_render_png(nap, nap_len_i, width, height, null, 0);
    if (required < 0) {
        std.debug.print("render failed (query): {d}\n", .{required});
        return 1;
    }

    const png = @as([*]u8, @ptrCast(c.malloc(@intCast(required)) orelse return 1));
    defer c.free(png);

    const written = naplps_render_png(nap, nap_len_i, width, height, png, required);
    if (written < 0) {
        std.debug.print("render failed: {d}\n", .{written});
        return 1;
    }

    if (!writeFile(out_path, png, @intCast(written))) {
        std.debug.print("cannot write {s}\n", .{out_path});
        return 1;
    }
    std.debug.print("Wrote {s} ({d} bytes, {d}x{d})\n", .{ out_path, written, width, height });

    return 0;
}
