// tools/aot/zig/build.zig
//
// Build: zig build
// Run:   ./zig-out/bin/naplps_demo ../../../Examples/telidraw/hello.nap hello.png

const std = @import("std");

pub fn build(b: *std.Build) void {
    const target = b.standardTargetOptions(.{});
    const optimize = b.standardOptimizeOption(.{});

    const mod = b.createModule(.{
        .root_source_file = b.path("main.zig"),
        .target = target,
        .optimize = optimize,
        .link_libc = true,
    });

    // Link against NAPLPS.dll / libNAPLPS.so / libNAPLPS.dylib in ../publish.
    // In Zig 0.16 library paths and linker lookups live on the Module, not the
    // Compile step.
    mod.addLibraryPath(b.path("../publish"));
    mod.linkSystemLibrary("NAPLPS", .{});

    const exe = b.addExecutable(.{
        .name = "naplps_demo",
        .root_module = mod,
    });

    b.installArtifact(exe);
}
