#Requires -Version 5.1
<#
.SYNOPSIS
    Builds the InescapableTarkovsSoftcore server mod and assembles the MO2
    overlay artifact tree.

.DESCRIPTION
    Produces build/overlay/SPT_Runtime/user/mods/com.sammeow.inescapable-softcore/
    containing the mod DLL, a copy of the controlled default config template
    (config/default-config.json), and a Resources/ placeholder directory.
    Idempotent: the overlay root is recreated on every run. Any failure results
    in a non-zero exit code.

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

    Write-Host "[ITS] Assembling overlay at $modDir..." -ForegroundColor Cyan
    if (Test-Path -LiteralPath $overlayRoot) {
        Remove-Item -LiteralPath $overlayRoot -Recurse -Force
    }

    $resourcesDir = Join-Path $modDir 'Resources'
    New-Item -ItemType Directory -Path $resourcesDir -Force | Out-Null

    Copy-Item -LiteralPath $dllPath -Destination $modDir -Force

    # config.json 为受控模板 config/default-config.json 的逐字节拷贝（JSONC，运行时容忍注释）
    Copy-Item -LiteralPath $configTemplate -Destination (Join-Path $modDir 'config.json') -Force

    # Resources/ is a placeholder until data assets land in a later ticket.
    New-Item -ItemType File -Path (Join-Path $resourcesDir '.gitkeep') -Force | Out-Null

    Write-Host "[ITS] Overlay ready: $modDir" -ForegroundColor Green
    Write-Host "[ITS] Deploy mapping: overlay root -> game root SPT_5xx; server mod -> SPT_Runtime\user\mods\" -ForegroundColor Green
    exit 0
}
catch {
    [Console]::Error.WriteLine("[ITS] BUILD FAILED: $($_.Exception.Message)")
    exit 1
}
