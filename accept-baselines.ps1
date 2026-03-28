param(
    [string]$File,
    [switch]$All,
    [switch]$NewOnly,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$baselinesDir = Join-Path (Join-Path (Join-Path $scriptDir "NAPLPSTests") "Visual") "Baselines"

# Find actuals directory - check common locations
$actualsDir = $null
$candidates = @(
    (Join-Path (Join-Path (Join-Path (Join-Path (Join-Path $scriptDir "NAPLPSTests") "bin") "Debug") "net10.0") "VisualRegression" | Join-Path -ChildPath "Actuals"),
    (Join-Path (Join-Path (Join-Path (Join-Path (Join-Path $scriptDir "NAPLPSTests") "bin") "Release") "net10.0") "VisualRegression" | Join-Path -ChildPath "Actuals")
)

foreach ($candidate in $candidates)
{
    if (Test-Path $candidate)
    {
        $actualsDir = $candidate
        break
    }
}

if (-not $actualsDir)
{
    Write-Error "Could not find Actuals directory. Run visual regression tests first: dotnet test --filter VisualBaseline"
    exit 1
}

Write-Host "Actuals:   $actualsDir"
Write-Host "Baselines: $baselinesDir"
Write-Host ""

$copied = 0

if ($File)
{
    $src = Join-Path $actualsDir $File
    $dst = Join-Path $baselinesDir $File

    if (-not (Test-Path $src))
    {
        Write-Error "Actual not found: $src"
        exit 1
    }

    if ($DryRun)
    {
        Write-Host "[DRY RUN] Would copy: $File"
    }
    else
    {
        $dstDir = Split-Path -Parent $dst
        if (-not (Test-Path $dstDir)) { New-Item -ItemType Directory -Path $dstDir -Force | Out-Null }
        Copy-Item $src $dst -Force
        Write-Host "Copied: $File"
    }

    $copied = 1
}
elseif ($All -or $NewOnly)
{
    $actuals = Get-ChildItem -Path $actualsDir -Recurse -File

    foreach ($actual in $actuals)
    {
        $relativePath = $actual.FullName.Substring($actualsDir.Length + 1)
        $baselinePath = Join-Path $baselinesDir $relativePath

        if ($NewOnly -and (Test-Path $baselinePath))
        {
            continue
        }

        if ($DryRun)
        {
            $action = if (Test-Path $baselinePath) { "overwrite" } else { "create" }
            Write-Host "[DRY RUN] Would $action`: $relativePath"
        }
        else
        {
            $dstDir = Split-Path -Parent $baselinePath
            if (-not (Test-Path $dstDir)) { New-Item -ItemType Directory -Path $dstDir -Force | Out-Null }
            Copy-Item $actual.FullName $baselinePath -Force
            $action = if (Test-Path $baselinePath) { "Updated" } else { "Created" }
            Write-Host "$action`: $relativePath"
        }

        $copied++
    }
}
else
{
    Write-Host "Usage:"
    Write-Host "  accept-baselines.ps1 -All              # Accept all actuals as new baselines"
    Write-Host "  accept-baselines.ps1 -NewOnly           # Accept only files without existing baselines"
    Write-Host "  accept-baselines.ps1 -File <path.apng>  # Accept a single file"
    Write-Host "  Add -DryRun to preview without copying"
    exit 0
}

Write-Host ""

if ($DryRun)
{
    Write-Host "$copied file(s) would be copied."
}
else
{
    Write-Host "$copied file(s) copied."
    Write-Host ""
    Write-Host "To commit:"
    Write-Host "  git add NAPLPSTests/Visual/Baselines/"
    Write-Host "  git commit -m `"Update visual regression baselines`""
}
