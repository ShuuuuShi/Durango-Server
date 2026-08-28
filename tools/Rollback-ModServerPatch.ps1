[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ServerPath
)

$ErrorActionPreference = 'Stop'
$target = (Resolve-Path -LiteralPath $ServerPath).Path
$recordPath = Join-Path $target 'mod-patch.json'
if (-not (Test-Path -LiteralPath $recordPath -PathType Leaf)) { throw "mod-patch.json was not found in '$target'" }
if (Get-Process -Name DurangoServer -ErrorAction SilentlyContinue) { throw 'Stop DurangoServer before rollback.' }
$record = Get-Content -LiteralPath $recordPath -Raw | ConvertFrom-Json
$backup = [IO.Path]::GetFullPath([IO.Path]::Combine($target, $record.backup))
$targetRoot = [IO.Path]::GetFullPath($target)
if (-not $backup.StartsWith((Join-Path $targetRoot '.durango-mod-backup'), [StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe backup path.' }
if (-not (Test-Path -LiteralPath $backup -PathType Container)) { throw "Backup was not found: $backup" }
foreach ($file in $record.files) {
    $from = Join-Path $backup $file
    if (Test-Path -LiteralPath $from -PathType Leaf) { Copy-Item -LiteralPath $from -Destination (Join-Path $target $file) -Force }
}
Remove-Item -LiteralPath $recordPath -Force
Write-Host "Rollback completed from $backup"
