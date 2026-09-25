#Requires -Version 5.1
<#
.SYNOPSIS
    Builds the InescapableTarkovsSoftcore server mod and assembles the MO2
    overlay artifact tree.

.DESCRIPTION
    Produces build/overlay/SPT_Runtime/user/mods/com.sammeow.inescapable-softcore/
    containing the mod DLL, a copy of the controlled default config template
    (config/default-config.json), and a Resources/ placeholder directory.
    When samuelTweaks.customBackground is enabled in the template, also deploys
    assets/launcher/bg.png to
    build/overlay/SPT_Runtime/SPT_Data/images/launcher/bg.png
    (SPT 5 ImageRouteImporter scans SPT_Data/images/ and serves it as
    /files/launcher/bg, the launcher background route).
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
$launcherBgAsset = Join-Path $repoRoot 'assets\launcher\bg.png'
# 启动器背景部署路径：overlay 根映射到游戏根，SPT5 从 SPT_Data\images\launcher\ 提供 /files/launcher/bg 路由
$launcherBgDir = Join-Path $overlayRoot 'SPT_Runtime\SPT_Data\images\launcher'

<#
.SYNOPSIS
    Reads samuelTweaks.customBackground / samuelTweaks.enabled from the JSONC
    config template. Missing keys default to enabled.
.NOTES
    Fail-closed: if the template cannot be parsed, warn and SKIP background
    deployment (never deploy on an unreadable template).
    The template is a controlled repo file: comment stripping only removes
    full-line / inline // comments and trailing commas. Naive-parse boundary:
    a string value containing // would be mis-stripped, so config values MUST
    NOT contain // (none do today).
#>
function Test-CustomBackgroundEnabled {
    param([Parameter(Mandatory)][string]$Path)

    $text = Get-Content -LiteralPath $Path -Raw
    $text = [regex]::Replace($text, '(?m)//.*$', '')
    $text = [regex]::Replace($text, ',(\s*[}\]])', '$1')

    try {
        $config = $text | ConvertFrom-Json
    }
    catch {
        Write-Warning "[ITS] 解析配置模板读取 customBackground 失败，跳过启动器背景部署（fail-closed）：$($_.Exception.Message)"
        return $false
    }

    if ($config.PSObject.Properties.Name -notcontains 'samuelTweaks') {
        return $true
    }

    $tweaks = $config.samuelTweaks
    if ($null -eq $tweaks) {
        return $true
    }

    if ($tweaks.PSObject.Properties.Name -contains 'enabled' -and -not [bool]$tweaks.enabled) {
        return $false
    }

    if ($tweaks.PSObject.Properties.Name -contains 'customBackground') {
        return [bool]$tweaks.customBackground
    }

    return $true
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

    # 启动器背景（G1 customBackground 语义）：静态件按 SPT5 真实路径投影到 SPT_Data\images\launcher\bg.png
    if (Test-CustomBackgroundEnabled -Path $configTemplate) {
        if (-not (Test-Path -LiteralPath $launcherBgAsset)) {
            throw "Launcher background asset not found: $launcherBgAsset"
        }

        New-Item -ItemType Directory -Path $launcherBgDir -Force | Out-Null
        Copy-Item -LiteralPath $launcherBgAsset -Destination (Join-Path $launcherBgDir 'bg.png') -Force
        Write-Host "[ITS] Launcher background deployed: $(Join-Path $launcherBgDir 'bg.png')" -ForegroundColor Green
    }
    else {
        Write-Host "[ITS] samuelTweaks.customBackground disabled; launcher background skipped" -ForegroundColor Yellow
    }

    Write-Host "[ITS] Overlay ready: $modDir" -ForegroundColor Green
    Write-Host "[ITS] Deploy mapping: overlay root -> game root SPT_5xx; server mod -> SPT_Runtime\user\mods\" -ForegroundColor Green
    exit 0
}
catch {
    [Console]::Error.WriteLine("[ITS] BUILD FAILED: $($_.Exception.Message)")
    exit 1
}
