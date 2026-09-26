#Requires -Version 5.1
<#
.SYNOPSIS
    Builds the InescapableTarkovsSoftcore server mod and assembles the MO2
    overlay artifact tree.

.DESCRIPTION
    Produces build/overlay/SPT_Runtime/user/mods/com.sammeow.inescapable-softcore/
    containing the mod DLL, the default config (config/default-config.json) and
    the data tables (data/**).

    Copy policy (upgrade-safe):
      - DLL: always overwritten.
      - config.json: copy-if-missing (existing player edits are preserved).
      - data/**: copy-if-missing per file (existing player edits are preserved).

    The overlay root is NOT wiped on rerun; only missing artifacts are added and
    the DLL is refreshed. Idempotent. Any failure results in a non-zero exit code.

.PARAMETER Configuration
    Build configuration. Default: Release.

.PARAMETER SptRuntimeDir
    Optional override for the SPT runtime reference-assembly directory,
    forwarded to MSBuild as -p:SptRuntimeDir=...

.EXAMPLE
    pwsh -File scripts/build.ps1
    pwsh -File scripts/build.ps1 -Configuration Debug -SptRuntimeDir "D:\SPT_Runtime"
#>
[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string]$SptRuntimeDir
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot    = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot 'src\InescapableTarkovsSoftcore\InescapableTarkovsSoftcore.csproj'
$assembly    = 'InescapableTarkovsSoftcore'
$modDirName  = 'com.sammeow.inescapable-softcore'
$overlayRoot = Join-Path $repoRoot 'build\overlay'
$modDir      = Join-Path $overlayRoot "SPT_Runtime\user\mods\$modDirName"
$dllPath     = Join-Path $repoRoot "src\InescapableTarkovsSoftcore\bin\$Configuration\net10.0\$assembly.dll"
$configTemplate = Join-Path $repoRoot 'config\default-config.json'
$dataRoot    = Join-Path $repoRoot 'data'

<#
.SYNOPSIS
    Recursively copies $Source into $Destination, skipping files that already exist.
.NOTES
    Copy-if-missing: existing (player-edited) files are never overwritten.
#>
function Copy-DataTree {
    param(
        [Parameter(Mandatory)][string]$Source,
        [Parameter(Mandatory)][string]$Destination
    )

    New-Item -ItemType Directory -Path $Destination -Force | Out-Null
    Get-ChildItem -LiteralPath $Source -Recurse -File | ForEach-Object {
        $relative = $_.FullName.Substring($Source.Length).TrimStart('\')
        $target = Join-Path $Destination $relative
        New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
        if (-not (Test-Path -LiteralPath $target)) {
            Copy-Item -LiteralPath $_.FullName -Destination $target -Force
            Write-Host "[ITS] data + $relative"
        }
    }
}

try {
    Write-Host "[ITS] Building $assembly ($Configuration)..." -ForegroundColor Cyan
    $buildArgs = @($projectPath, '-c', $Configuration)
    if ($SptRuntimeDir) {
        $buildArgs += "-p:SptRuntimeDir=$SptRuntimeDir"
    }

    & dotnet build @buildArgs
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed with exit code $LASTEXITCODE"
    }

    if (-not (Test-Path -LiteralPath $dllPath)) {
        throw "Build output not found: $dllPath"
    }

    if (-not (Test-Path -LiteralPath $configTemplate)) {
        throw "Config template not found: $configTemplate"
    }

    if (-not (Test-Path -LiteralPath $dataRoot)) {
        throw "Data root not found: $dataRoot"
    }

    Write-Host "[ITS] Assembling overlay at $modDir..." -ForegroundColor Cyan
    New-Item -ItemType Directory -Path $modDir -Force | Out-Null

    # DLL 始终覆盖
    Copy-Item -LiteralPath $dllPath -Destination $modDir -Force

    # config.json：copy-if-missing（保留玩家既有编辑）
    $configDest = Join-Path $modDir 'config.json'
    if (-not (Test-Path -LiteralPath $configDest)) {
        Copy-Item -LiteralPath $configTemplate -Destination $configDest -Force
        Write-Host "[ITS] config.json + (from template)"
    }
    else {
        Write-Host "[ITS] config.json 已存在，保留（copy-if-missing）" -ForegroundColor Yellow
    }

    # data/**：逐文件 copy-if-missing
    Copy-DataTree -Source $dataRoot -Destination (Join-Path $modDir 'data')

    Write-Host "[ITS] Overlay ready: $modDir" -ForegroundColor Green
    Write-Host "[ITS] Deploy mapping: overlay root -> game root SPT_5xx; server mod -> SPT_Runtime\user\mods\" -ForegroundColor Green
    exit 0
}
catch {
    [Console]::Error.WriteLine("[ITS] BUILD FAILED: $($_.Exception.Message)")
    exit 1
}
