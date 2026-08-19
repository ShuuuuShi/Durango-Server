# drive-game.ps1 - sang hai tua-lakhon nai kem tham ngan eng (mai chai mouse)
#
#   powershell -File tools\drive-game.ps1 -List
#   powershell -File tools\drive-game.ps1 -Scenario cook
#   powershell -File tools\drive-game.ps1 -Scenario cook -Target "MyName" -Shots
#   powershell -File tools\drive-game.ps1 -Cmd "walk 40 177","place bonfire","craft skewer"
#
# WHY THIS EXISTS
#   Testing the real client used to mean driving the mouse, which steals the desktop and
#   breaks the moment a window moves. This drives the character through the SERVER instead:
#   every command goes to the player who is already logged in, through the same packet
#   handlers a real click would hit. No mouse, no keyboard, no focus stealing.
#   The player just watches their character do the work.
#
#   Screenshots (-Shots) capture the game window only and never touch the cursor.
#
# NOTE: ASCII only - Windows PowerShell 5.1 reads .ps1 files as ANSI.

param(
  [string]$Scenario = "",
  [string[]]$Cmd = @(),
  [string]$Target = "",
  [string]$GameHost = "127.0.0.1",
  [int]$GamePort = 8191,
  [string]$Bot = "gm",
  [switch]$Shots,
  [switch]$List
)

$ErrorActionPreference = "Stop"
$root   = Split-Path -Parent $PSScriptRoot
$tester = Join-Path $root "test-client"
$shotDir = Join-Path $root "shots"

# ---------------------------------------------------------------- scenarios
#
# Each scenario is a list of steps: either "<control verb> [args]" or "wait <seconds>".
# Steps run against the logged-in player, in order.

$Scenarios = [ordered]@{
  "status" = @(
    "status",
    "bag",
    "prof"
  )

  # Full cooking loop: stand somewhere clear, drop a fire, grill meat, eat it.
  "cook" = @(
    "give cook",
    "wait 2",
    "bag",
    "place bonfire",
    "wait 4",
    "craft skewer",
    "wait 5",
    "bag",
    "eat meat",
    "wait 3",
    "status"
  )

  # Proves the workbench gate: broth needs the BIG fire, not the small one.
  "cook-tier" = @(
    "give cook",
    "wait 2",
    "place bonfire",
    "wait 4",
    "craft broth",
    "wait 4",
    "place bonfire_01",
    "wait 4",
    "craft broth",
    "wait 5",
    "bag"
  )

  # Gather in a loop and watch the gathering proficiency climb.
  "gather" = @(
    "prof",
    "gather", "wait 3",
    "gather", "wait 3",
    "gather", "wait 3",
    "gather", "wait 3",
    "gather", "wait 3",
    "gather", "wait 3",
    "prof"
  )

  # Craft repeatedly and watch the weaponcrafting proficiency climb.
  "craft-loop" = @(
    "give stone",
    "wait 2",
    "prof",
    "craft blade_stone", "wait 3",
    "craft blade_stone", "wait 3",
    "craft blade_stone", "wait 3",
    "craft blade_stone", "wait 3",
    "prof",
    "bag"
  )

  # Hunt: spawn something next to us, kill it, butcher it.
  "hunt" = @(
    "give knife",
    "wait 1",
    "spawn",
    "wait 2",
    "attack", "wait 3",
    "attack", "wait 3",
    "attack", "wait 3",
    "gather", "wait 3",
    "prof",
    "bag"
  )
}

if ($List) {
  Write-Output "scenarios:"
  foreach ($k in $Scenarios.Keys) {
    Write-Output ("  {0,-12} {1} steps" -f $k, $Scenarios[$k].Count)
  }
  Write-Output ""
  Write-Output "control verbs: tp walk stop gather attack craft eat place bag prof give heal kill spawn say status"
  exit 0
}

# ---------------------------------------------------------------- pick the target player
#
# Default target = the one human player online (not our own bot). Asking the server beats
# making the user type their character name, which is Thai text in a PowerShell argument.

function Get-OnlinePlayers {
  Push-Location $tester
  try {
    $out = & dotnet run --no-build -- --console $GameHost $GamePort $Bot --cmd "cheat who" 2>&1
  } finally {
    Pop-Location
  }
  $ids = @()
  foreach ($line in $out) {
    # "  <name> | <entityId> | tile x,y | lvN"
    if ($line -match '^\s{2,}(.+?)\s\|\s(\S+)\s\|\stile') {
      $name = $Matches[1].Trim()
      $id   = $Matches[2].Trim()
      if ($id -ne $Bot) { $ids += [pscustomobject]@{ Name = $name; Id = $id } }
    }
  }
  return $ids
}

if (-not $Target) {
  $players = @(Get-OnlinePlayers)
  if ($players.Count -eq 0) {
    Write-Output "no player online - start the game and connect first (menu 1), then run this again"
    exit 1
  }
  if ($players.Count -gt 1) {
    Write-Output "more than one player online - pass -Target <name or entityId>:"
    foreach ($p in $players) { Write-Output ("  {0}  ({1})" -f $p.Name, $p.Id) }
    exit 1
  }
  $Target = $players[0].Id
  Write-Output ("target: {0}  ({1})" -f $players[0].Name, $Target)
}

# ---------------------------------------------------------------- build the command list

$steps = @()
if ($Cmd.Count -gt 0) {
  $steps = $Cmd
} elseif ($Scenario) {
  if (-not $Scenarios.Contains($Scenario)) {
    Write-Output "unknown scenario '$Scenario' - run with -List to see them"
    exit 1
  }
  $steps = $Scenarios[$Scenario]
} else {
  Write-Output "pass -Scenario <name> or -Cmd '<verb> [args]',... (or -List)"
  exit 1
}

$consoleArgs = @("run", "--no-build", "--", "--console", $GameHost, "$GamePort", $Bot)
foreach ($step in $steps) {
  if ($step -match '^\s*wait\s') {
    $consoleArgs += @("--cmd", $step)
  } else {
    $consoleArgs += @("--cmd", "control $Target $step")
  }
}

# ---------------------------------------------------------------- screenshot (window only, no cursor)

function Save-GameShot([string]$tag) {
  $proc = @(Get-Process -Name DurangoV2 -ErrorAction SilentlyContinue)
  if ($proc.Count -eq 0) { return }
  if (-not (Test-Path $shotDir)) { New-Item -ItemType Directory -Force $shotDir | Out-Null }
  Add-Type -AssemblyName System.Windows.Forms, System.Drawing
  $prevEap = $ErrorActionPreference
  $ErrorActionPreference = 'Continue'
  if (-not ("DG" -as [type])) {
    Add-Type @"
using System;
using System.Runtime.InteropServices;
public class DG {
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int L, T, R, B; }
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
}
"@
  }
  $ErrorActionPreference = $prevEap
  $h = $proc[0].MainWindowHandle
  if ($h -eq [IntPtr]::Zero) { return }
  $r = New-Object DG+RECT
  if (-not [DG]::GetWindowRect($h, [ref]$r)) { return }
  $w = $r.R - $r.L; $ht = $r.B - $r.T
  if ($w -le 0 -or $ht -le 0) { return }
  $bmp = New-Object System.Drawing.Bitmap $w, $ht
  $g = [System.Drawing.Graphics]::FromImage($bmp)
  $g.CopyFromScreen($r.L, $r.T, 0, 0, $bmp.Size)
  $out = Join-Path $shotDir ("{0}-{1}.png" -f $tag, (Get-Date -Format "HHmmss"))
  $bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
  $g.Dispose(); $bmp.Dispose()
  Write-Output "  shot -> $out"
}

# ---------------------------------------------------------------- run

$label = if ($Scenario) { $Scenario } else { "custom" }
Write-Output "running '$label' on $Target ($($steps.Count) steps)"
if ($Shots) { Save-GameShot "$label-before" }

Push-Location $tester
try {
  & dotnet @consoleArgs
} finally {
  Pop-Location
}

if ($Shots) { Save-GameShot "$label-after" }
Write-Output "done"
