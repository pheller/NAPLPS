# tools/aot/c/build-msvc.ps1
#
# Build the C example using cl.exe (MSVC). Call from any shell; uses VsDevCmd
# under the hood to locate the compiler and SDK. The equivalent Makefile targets
# gcc/MinGW; use whichever fits your environment.
#
# Usage:
#     pwsh tools/aot/c/build-msvc.ps1
#     .\naplps_demo.exe ..\..\..\Examples\telidraw\hello.nap hello.png

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$includeDir = (Resolve-Path (Join-Path $scriptDir "..\include")).Path
$publishDir = (Resolve-Path (Join-Path $scriptDir "..\publish")).Path

# Locate MSVC via vswhere.
$vswhere = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"
if (-not (Test-Path $vswhere)) { throw "vswhere.exe not found. Install VS Build Tools." }
$vsPath = & $vswhere -latest -property installationPath
$msvcDir = Get-ChildItem (Join-Path $vsPath "VC\Tools\MSVC") | Sort-Object Name -Descending | Select-Object -First 1
$cl = Join-Path $msvcDir.FullName "bin\Hostx64\x64\cl.exe"

# Locate Windows SDK.
$sdkRoot = "C:\Program Files (x86)\Windows Kits\10"
$sdkVersion = Get-ChildItem (Join-Path $sdkRoot "Include") | Sort-Object Name -Descending | Select-Object -First 1
$sdkIncDir = Join-Path $sdkRoot "Include\$($sdkVersion.Name)"
$sdkLibDir = Join-Path $sdkRoot "Lib\$($sdkVersion.Name)"

Push-Location $scriptDir
try
{
    $args = @(
        "/nologo",
        "/I$includeDir",
        "/I$($msvcDir.FullName)\include",
        "/I$sdkIncDir\ucrt",
        "/I$sdkIncDir\shared",
        "/I$sdkIncDir\um",
        "main.c",
        "/link",
        "/LIBPATH:$($msvcDir.FullName)\lib\x64",
        "/LIBPATH:$sdkLibDir\ucrt\x64",
        "/LIBPATH:$sdkLibDir\um\x64",
        "/LIBPATH:$publishDir",
        "NAPLPS.lib",
        "/OUT:naplps_demo.exe"
    )

    & $cl @args
    if ($LASTEXITCODE -ne 0) { throw "cl.exe failed" }

    # The .exe needs NAPLPS.dll next to it at runtime.
    Copy-Item (Join-Path $publishDir "NAPLPS.dll") . -Force
    Write-Host ""
    Write-Host "Built: naplps_demo.exe"
}
finally
{
    Pop-Location
}
