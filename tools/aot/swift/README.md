# Swift binding 🦅

Requires the Swift toolchain on PATH (`swift --version`):

- **Windows**: `winget install Swift.Toolchain` or download from https://www.swift.org/install/windows/
- **macOS**: ships with Xcode; `xcode-select --install` for the command-line tools
- **Linux**: download the tarball from https://www.swift.org/install/linux/

## Build and run 🛠️

### macOS / Linux

```bash
# 1. Publish NAPLPS.dll / libNAPLPS.so first (one level up)
pwsh ../publish.ps1

# 2. Build and run
swift build -c release
./.build/release/naplps_demo ../../../Examples/telidraw/hello.nap hello.png
```

The linker rpath set by `Package.swift`'s `linkerSettings` handles `libNAPLPS.{dylib,so}` discovery.

### Windows

Swift on Windows needs three things before `swift build` will link:

1. Swift runtime DLLs on `PATH` (`Foundation.dll`, `Dispatch.dll`, `BlocksRuntime.dll`, …)
2. `SDKROOT` pointing at the Platform SDK directory
3. The VS **x64** linker on PATH (vcvars64.bat must be sourced; otherwise `msvcrt.lib` picks up its x86 variant and you get `machine type x86 conflicts with x64`)

`build-windows.bat` does all three:

```powershell
cd tools/aot/swift
cmd /c build-windows.bat
copy ..\publish\NAPLPS.dll .\.build\x86_64-unknown-windows-msvc\release\
$env:PATH = "$env:LOCALAPPDATA\Programs\Swift\Runtimes\6.3.1\usr\bin;" + $env:PATH
.\.build\x86_64-unknown-windows-msvc\release\naplps_demo.exe ..\..\..\Examples\telidraw\hello.nap hello.png
```

The batch file hardcodes the VS 2026 Enterprise location and Swift 6.3.1 paths; adjust if your install differs.

## How it works 🔍

- `Sources/CNaplps/module.modulemap` declares `CNaplps` as a system module that includes the C header and links the `NAPLPS` library.
- `Sources/CNaplps/shim.h` is a one-liner that `#include`s `../../../include/naplps.h` so SwiftPM's module system sees it as a local file.
- `Sources/naplps_demo/main.swift` calls the C functions through the `CNaplps` module. Signatures come from the header, Swift generates the bridging automatically.

No `@_silgen_name`, no manual C interop wrappers. SwiftPM's system-library target is the idiomatic way to wrap a C library.

## Why this is useful 💡

Swift for Mac and iOS apps that want to render `.nap` files in their archival viewer UIs. Pair with SwiftUI + `NSImage`/`UIImage` (decode the PNG returned by `naplps_render_png` into an image view). Single SwiftPM package covers Mac command-line + iOS app + Mac app if you factor the bridging out into a shared module.
