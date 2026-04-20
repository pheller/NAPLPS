@echo off
REM Swift build wrapper that sets up VS x64 environment first. Needed because
REM swiftc on Windows pulls in MSVC libs via the linker, and the ambient shell
REM PATH may have the x86 MSVC toolchain first (which causes machine-type
REM conflicts when linking x64). Running vcvars64.bat fixes LIB/INCLUDE/PATH
REM to the x64 flavors before SwiftPM invokes the linker.

call "C:\Program Files\Microsoft Visual Studio\18\Enterprise\VC\Auxiliary\Build\vcvars64.bat"
if errorlevel 1 exit /b 1

set "PATH=C:\Users\Fox\AppData\Local\Programs\Swift\Runtimes\6.3.1\usr\bin;C:\Users\Fox\AppData\Local\Programs\Swift\Toolchains\6.3.1+Asserts\usr\bin;%PATH%"
set "SDKROOT=C:\Users\Fox\AppData\Local\Programs\Swift\Platforms\6.3.1\Windows.platform\Developer\SDKs\Windows.sdk"

swift build -c release %*
