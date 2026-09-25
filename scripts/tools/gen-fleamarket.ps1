#requires -version 5.1
<#
.SYNOPSIS
    Regenerate data/softcore/fleamarket.json from the source mod's TypeScript assets.

.DESCRIPTION
    Parses fleamarket.ts / keys.ts / itemBaseClasses.ts from the (read-only) source mod and
    resolves symbolic enum names (ItemTpl.* / BaseClasses.*) against a symbol dump produced
    from the SPT 5 runtime assemblies. See data/softcore/MANIFEST.md for provenance.

    Required input: -SymbolsJson, a JSON file { "ItemTpl": {name: hex}, "BaseClasses": {name: hex} }.
    The dump is produced by scripts/tools/dump-spt-symbols.cs (compile against SPT_5xx\SPT_Runtime).

.EXAMPLE
    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/tools/gen-fleamarket.ps1 -SymbolsJson D:\Temp\spt-symbols.json
#>
[CmdletBinding()]
param(
    [string]$SymbolsJson = 'D:\Temp\opencode\sptprobe09\symbols.json',
    [string]$ModsRoot = 'E:\Game\EFT_Offline\Life_in_Norvinsk_v0.3.2\mods'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$srcDir = Get-ChildItem -LiteralPath $ModsRoot -Directory |
    ForEach-Object { Join-Path $_.FullName 'user\mods\odt-softcore\src\assets' } |
    Where-Object { Test-Path -LiteralPath $_ } |
    Select-Object -First 1
if (-not $srcDir) { throw 'source assets dir not found' }
if (-not (Test-Path -LiteralPath $SymbolsJson)) { throw "symbols json not found: $SymbolsJson" }

$symbols = Get-Content -LiteralPath $SymbolsJson -Raw | ConvertFrom-Json
$itemTpl = @{}
foreach ($p in $symbols.ItemTpl.PSObject.Properties) { $itemTpl[$p.Name] = [string]$p.Value }
$baseClasses = @{}
foreach ($p in $symbols.BaseClasses.PSObject.Properties) { $baseClasses[$p.Name] = [string]$p.Value }

# Explicit renames where the source symbol no longer exists in SPT5.
$aliases = @{
    'ItemTpl.KEY_SHARED_BEDROOM_MARKED' = '62987dfc402c7f69bf010923' # renamed to KEY_SUBSTATION_MARKED in SPT5
}

$script:missing = New-Object System.Collections.Generic.List[string]

function Read-Text($name) { Get-Content -LiteralPath (Join-Path $srcDir $name) -Raw }

function Get-ArrayBody($text, $name) {
    $m = [regex]::Match($text, "export const $name\s*=\s*\[([\s\S]*?)\r?\n\]")
    if (-not $m.Success) { throw "array $name not found" }
    return $m.Groups[1].Value
}

function Get-ObjectBody($text, $name) {
    $m = [regex]::Match($text, "export const $name\s*=\s*\{([\s\S]*?)\r?\n\}")
    if (-not $m.Success) { throw "object $name not found" }
    return $m.Groups[1].Value
}

function Resolve-Symbol($kind, $name) {
    $table = if ($kind -eq 'ItemTpl') { $itemTpl } else { $baseClasses }
    if ($table.ContainsKey($name)) { return $table[$name] }

    # fallback: normalized match (SPT5 renames e.g. MEDKIT -> MED_KIT)
    $norm = $name.Replace('_', '').ToLowerInvariant()
    $hits = @($table.Keys | Where-Object { $_.Replace('_', '').ToLowerInvariant() -eq $norm })
    if ($hits.Count -eq 1) { return $table[$hits[0]] }

    $script:missing.Add("$kind.$name")
    return $null
}

function Resolve-Token($raw) {
    $t = $raw.Trim().TrimEnd(',').Trim()
    if ($t.StartsWith('"') -or $t.StartsWith("'")) { return $t.Trim('"', "'") }
    if ($aliases.ContainsKey($t)) { return $aliases[$t] }
    if ($t -match '^ItemTpl\.(.+)$') { return Resolve-Symbol 'ItemTpl' $Matches[1] }
    if ($t -match '^BaseClasses\.(.+)$') { return Resolve-Symbol 'BaseClasses' $Matches[1] }
    throw "unresolvable token: $t"
}

function Parse-Array($body) {
    $out = New-Object System.Collections.Generic.List[string]
    foreach ($line in ($body -split "`n")) {
        $noComment = ($line -split '//')[0].Trim()
        if ($noComment -eq '' -or $noComment -eq ',') { continue }
        $resolved = Resolve-Token $noComment
        if ($null -ne $resolved) { $out.Add($resolved) }
    }
    return $out.ToArray()
}

function Parse-PriceObject($body) {
    $map = [ordered]@{}
    foreach ($line in ($body -split "`n")) {
        $noComment = ($line -split '//')[0].Trim()
        if ($noComment -eq '') { continue }
        $m = [regex]::Match($noComment, '^"([^"]+)"\s*:\s*(-?\d+(?:\.\d+)?)\s*,?$')
        if ($m.Success) { $map[$m.Groups[1].Value] = [double]$m.Groups[2].Value }
    }
    return $map
}

$fm = Read-Text 'fleamarket.ts'
$keys = Read-Text 'keys.ts'
$ibc = Read-Text 'itemBaseClasses.ts'

$data = [ordered]@{
    whitelist                      = @(Parse-Array (Get-ArrayBody $fm 'whitelist'))
    actualBaseClasses              = @(Parse-Array (Get-ArrayBody $fm 'actualBaseClasses'))
    fleaBarterRequestWhitelist     = @(Parse-Array (Get-ArrayBody $fm 'fleaBarterRequestWhitelist'))
    requestWhitelist               = (Parse-PriceObject (Get-ObjectBody $fm 'requestWhitelist'))
    fleaListingsWhitelistHandBook  = @(Parse-Array (Get-ArrayBody $fm 'fleaListingsWhitelistHandBook'))
    pacifistFenceItemBaseWhitelist = @(Parse-Array (Get-ArrayBody $fm 'pacifistFenceItemBaseWhitelist'))
    bsgBlacklist                   = @(Parse-Array (Get-ArrayBody $fm 'BSGblacklist'))
    itemBaseClasses                = @(Parse-Array (Get-ArrayBody $ibc 'itemBaseClasses'))
    questKeys                      = @(Parse-Array (Get-ArrayBody $keys 'questKeys'))
    markedKeys                     = @(Parse-Array (Get-ArrayBody $keys 'markedKeys'))
}

$json = $data | ConvertTo-Json -Depth 6
$outPath = Join-Path $repoRoot 'data\softcore\fleamarket.json'
[System.IO.File]::WriteAllText($outPath, $json + "`n", (New-Object System.Text.UTF8Encoding($false)))

foreach ($k in $data.Keys) { "$k = $($data[$k].Count)" }
if ($script:missing.Count -gt 0) { "MISSING SYMBOLS: " + (($script:missing | Sort-Object -Unique) -join ', ') }
