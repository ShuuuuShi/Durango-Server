[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter(Mandatory = $true)]
    [string]$ServerPath,
    [string]$BuildPath = (Join-Path $PSScriptRoot '..\server\bin\Release\net9.0'),
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
$target = (Resolve-Path -LiteralPath $ServerPath).Path
$source = (Resolve-Path -LiteralPath $BuildPath).Path
$requiredTarget = Join-Path $target 'DurangoServer.exe'
if (-not (Test-Path -LiteralPath $requiredTarget -PathType Leaf)) {
    throw "DurangoServer.exe was not found in '$target'. This patch requires our DurangoServer runtime."
}
if (Get-Process -Name DurangoServer -ErrorAction SilentlyContinue) {
    throw 'Stop DurangoServer before installing the patch.'
}

$files = @(
    'DurangoServer.exe', 'DurangoServer.dll', 'DurangoServer.deps.json', 'DurangoServer.runtimeconfig.json',
    'DurangoModSdk.dll', '0Harmony.dll', 'MsgPack.dll', 'Newtonsoft.Json.dll', 'Snappier.dll', 'System.CodeDom.dll'
)
foreach ($file in $files) {
    if (-not (Test-Path -LiteralPath (Join-Path $source $file) -PathType Leaf)) { throw "Build file is missing: $file" }
}

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$backup = Join-Path $target (Join-Path '.durango-mod-backup' $stamp)
$mods = Join-Path $target 'mods'
Write-Host "Source: $source"
Write-Host "Target: $target"
Write-Host "Backup: $backup"
if ($DryRun) { Write-Host '[dry-run] no files changed'; exit 0 }

New-Item -ItemType Directory -Path $backup -Force | Out-Null
foreach ($file in $files) {
    $destination = Join-Path $target $file
    if (Test-Path -LiteralPath $destination -PathType Leaf) { Copy-Item -LiteralPath $destination -Destination (Join-Path $backup $file) -Force }
    Copy-Item -LiteralPath (Join-Path $source $file) -Destination $destination -Force
}
New-Item -ItemType Directory -Path $mods -Force | Out-Null
$record = [ordered]@{
    installed_at_utc = [DateTime]::UtcNow.ToString('o')
    source = $source
    backup = $backup
    files = $files
    rollback = "Copy files from '$backup' back to '$target' after stopping DurangoServer"
}
$record | ConvertTo-Json -Depth 3 | Set-Content -LiteralPath (Join-Path $target 'mod-patch.json') -Encoding UTF8
Write-Host "Mod runtime patch installed. Put server mod DLLs/packages under '$mods'."
Write-Host "Rollback: stop the server, then copy files from '$backup' back to '$target'."
