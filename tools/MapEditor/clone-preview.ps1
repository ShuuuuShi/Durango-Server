# Clone the Durango runtime into an isolated preview directory.
# This script never modifies the source game, client, or server directories.
[CmdletBinding()]
param(
  [string]$Destination = '',
  [switch]$DryRun,
  [switch]$Resume
)

$ErrorActionPreference = 'Stop'

$mapEditorDir = [System.IO.Path]::GetFullPath($PSScriptRoot)
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $mapEditorDir '..\..'))
$sourceDirs = [ordered]@{
  game = Join-Path $repoRoot 'game'
  client = Join-Path $repoRoot 'client'
  'client-mod-sdk' = Join-Path $repoRoot 'client-mod-sdk'
  'mod-sdk' = Join-Path $repoRoot 'mod-sdk'
  server = Join-Path $repoRoot 'server'
}
if ([string]::IsNullOrWhiteSpace($Destination)) {
  $Destination = Join-Path $repoRoot '..\MapEditor-Preview'
}
$Destination = [System.IO.Path]::GetFullPath($Destination)

function Normalize-Path([string]$Value) {
  return ([System.IO.Path]::GetFullPath($Value)).TrimEnd('\')
}

function Is-SameOrChild([string]$Candidate, [string]$Parent) {
  $candidatePath = (Normalize-Path $Candidate) + '\'
  $parentPath = (Normalize-Path $Parent) + '\'
  return $candidatePath.StartsWith($parentPath, [System.StringComparison]::OrdinalIgnoreCase)
}

$sourceRoot = Normalize-Path $repoRoot
$destinationRoot = Normalize-Path $Destination
if ($destinationRoot -eq $sourceRoot -or (Is-SameOrChild $destinationRoot $sourceRoot)) {
  throw "Destination must be outside the repository root: $destinationRoot"
}
foreach ($entry in $sourceDirs.GetEnumerator()) {
  if (-not (Test-Path -LiteralPath $entry.Value -PathType Container)) {
    throw "Missing source directory '$($entry.Key)': $($entry.Value)"
  }
}
if ((Test-Path -LiteralPath $Destination) -and -not $Resume) {
  throw "Destination already exists. Refusing to overwrite it: $Destination"
}

function Get-DirectoryBytes([string]$Path) {
  $sum = (Get-ChildItem -LiteralPath $Path -File -Recurse -Force -ErrorAction Stop |
    Measure-Object -Property Length -Sum).Sum
  if ($null -eq $sum) { return [int64]0 }
  return [int64]$sum
}

function Get-CriticalFiles([string]$Root) {
  $relativeNames = @(
    'DurangoV2.exe',
    'DurangoUpdater.exe',
    'UnityPlayer.dll',
    'DurangoV2_Data/resources.assets',
    'DurangoV2_Data/StreamingAssets/AssetBundles/Info.5.2.1.json',
    'DurangoV2_Data/Managed/Assembly-CSharp.dll',
    'saves/world.json',
    'bin/verify/DurangoServer.dll'
  )
  $result = @()
  foreach ($relativeName in $relativeNames) {
    $filePath = Join-Path $Root ($relativeName -replace '/', '\')
    if (Test-Path -LiteralPath $filePath -PathType Leaf) {
      $hash = Get-FileHash -LiteralPath $filePath -Algorithm SHA256
      $result += [ordered]@{
        path = $relativeName
        bytes = (Get-Item -LiteralPath $filePath).Length
        sha256 = $hash.Hash.ToLowerInvariant()
      }
    }
  }
  return $result
}

$inventory = @()
$totalBytes = [int64]0
foreach ($entry in $sourceDirs.GetEnumerator()) {
  $bytes = Get-DirectoryBytes $entry.Value
  $fileCount = (Get-ChildItem -LiteralPath $entry.Value -File -Recurse -Force | Measure-Object).Count
  $totalBytes += $bytes
  $inventory += [ordered]@{
    name = $entry.Key
    source = $entry.Value
    files = $fileCount
    bytes = $bytes
  }
}

$drive = [System.IO.Path]::GetPathRoot($Destination)
$driveInfo = New-Object System.IO.DriveInfo($drive)
$requiredBytes = [int64]($totalBytes * 1.10)
Write-Host "Preview destination: $Destination"
Write-Host ("Source size: {0:N2} GB ({1:N0} bytes)" -f ($totalBytes / 1GB), $totalBytes)
Write-Host ("Safety headroom required: {0:N2} GB" -f ($requiredBytes / 1GB))
Write-Host ("Free space on {0}: {1:N2} GB" -f $drive, ($driveInfo.AvailableFreeSpace / 1GB))
$inventory | ForEach-Object { Write-Host ("  {0}: {1:N0} files, {2:N2} GB" -f $_.name, $_.files, ($_.bytes / 1GB)) }
if ($driveInfo.AvailableFreeSpace -lt $requiredBytes) {
  throw "Not enough free space for an isolated clone"
}

$manifest = [ordered]@{
  schema = 1
  createdAt = (Get-Date).ToUniversalTime().ToString('o')
  repositoryRoot = $repoRoot
  destinationRoot = $Destination
  sourceInventory = $inventory
  criticalSourceFiles = @{}
  copyPolicy = 'copy-only; no source deletion or mirroring'
}
foreach ($entry in $sourceDirs.GetEnumerator()) {
  $manifest.criticalSourceFiles[$entry.Key] = @(Get-CriticalFiles $entry.Value)
}

if ($DryRun) {
  Write-Host 'DRY RUN: no files copied.' -ForegroundColor Yellow
  $manifest | ConvertTo-Json -Depth 8
  exit 0
}

New-Item -ItemType Directory -Path $Destination -Force | Out-Null
foreach ($entry in $sourceDirs.GetEnumerator()) {
  $target = Join-Path $Destination $entry.Key
  New-Item -ItemType Directory -Path $target -Force | Out-Null
  & robocopy $entry.Value $target /E /COPY:DAT /DCOPY:DAT /R:1 /W:1 /XJ /NFL /NDL /NP | Out-Host
  if ($LASTEXITCODE -gt 7) {
    throw "Copy failed for $($entry.Key), robocopy exit code $LASTEXITCODE"
  }
}

$manifest.cloneCriticalFiles = @{}
foreach ($entry in $sourceDirs.GetEnumerator()) {
  $manifest.cloneCriticalFiles[$entry.Key] = @(Get-CriticalFiles (Join-Path $Destination $entry.Key))
}
$manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $Destination 'clone-manifest.json') -Encoding UTF8
Write-Host "Clone completed: $Destination" -ForegroundColor Green
Write-Host "Manifest: $(Join-Path $Destination 'clone-manifest.json')"
