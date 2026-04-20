# tools/aot/publish.ps1
#
# Publishes NAPLPS for NativeAOT and copies the produced .lib (import library)
# into the publish directory alongside the .dll. The AOT build writes the .lib
# into an intermediate native/ folder by default; we want it in publish/ so C/C++
# examples can link without extra path arguments.
#
# Usage:
#     pwsh tools/aot/publish.ps1 [-Rid win-x64]
#
# Prerequisites (Windows):
#   - Visual Studio 2026 with the C++ workload installed
#   - vswhere.exe on PATH (prepend C:\Program Files (x86)\Microsoft Visual Studio\Installer)

param(
    [string]$Rid = "win-x64"
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot  = Split-Path -Parent (Split-Path -Parent $scriptDir)
$csproj    = Join-Path $repoRoot "NAPLPS/NAPLPS.csproj"
$outDir    = Join-Path $scriptDir "publish"
$nativeDir = Join-Path $repoRoot "NAPLPS/bin/Release/net10.0/$Rid/native"

Write-Host "Publishing NAPLPS (AOT) for $Rid..."
dotnet publish $csproj -c Release -r $Rid --property:PublishAot=true -o $outDir
if ($LASTEXITCODE -ne 0) { throw "publish failed" }

# Copy the import library (.lib on Windows, not produced on Linux/mac)
if ($IsWindows -or ([System.Environment]::OSVersion.Platform -eq 'Win32NT'))
{
    $lib = Join-Path $nativeDir "NAPLPS.lib"
    $exp = Join-Path $nativeDir "NAPLPS.exp"
    if (Test-Path $lib)
    {
        Copy-Item $lib $outDir -Force
        Copy-Item $exp $outDir -Force
        Write-Host "Copied NAPLPS.lib + NAPLPS.exp to $outDir"
    }
    else
    {
        Write-Warning "Import library not found at $lib. C/C++ examples may fail to link."
    }
}

Write-Host ""
Write-Host "Publish output:"
Get-ChildItem $outDir | Format-Table Name, Length -AutoSize
