param(
  [switch]$NoInstall
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $PSScriptRoot 'MemoryBotMod/MemoryBotMod.csproj'
$gameMods = Join-Path $root 'game/mods'
& dotnet build $project -c Release -v quiet
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
if (-not $NoInstall) {
  New-Item -ItemType Directory -Force $gameMods | Out-Null
  $dll = Join-Path $PSScriptRoot 'MemoryBotMod/bin/Release/net35/DurangoMemoryBot.dll'
  Copy-Item $dll (Join-Path $gameMods 'DurangoMemoryBot.dll') -Force
  Write-Host "Installed DurangoMemoryBot.dll to $gameMods"
}
