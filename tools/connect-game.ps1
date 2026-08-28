# connect-game.ps1 - launch DurangoV2 and walk the UI to connect to a DurangoServer
#
#   powershell -File tools\connect-game.ps1                 # connect to 127.0.0.1
#   powershell -File tools\connect-game.ps1 -Ip 192.168.1.5
#   powershell -File tools\connect-game.ps1 -SkipLaunch     # game already running
#
# Why this exists: the "visit a friend's island" flow is the only way to point the
# retail client at our server, and it is 5 UI clicks deep. Doing it by hand every
# time the server restarts wastes minutes; this replays the exact click path.
#
# Window-relative coordinates below assume the default 827x544 window that
# game\launch.bat produces. If the window size changes, re-record them.
#
# NOTE: ASCII only - Windows PowerShell 5.1 reads .ps1 files as ANSI.

param(
  [string]$Ip = "127.0.0.1",
  [switch]$SkipLaunch,
  # use the game's Main menu -> Visit Friend's Island -> Direct Input flow
  [switch]$Manual,
  # the game boots to the title screen ("Multi Play Mode" + character + Start);
  # pass -InGame when the client is already standing on an island
  [switch]$InGame
)

$ErrorActionPreference = "Stop"
$gameDir = Join-Path (Split-Path -Parent $PSScriptRoot) "game"

Add-Type -AssemblyName System.Windows.Forms, System.Drawing
# Add-Type emits a harmless "System.Func defined in multiple assemblies" warning that
# $ErrorActionPreference='Stop' turns into a hard error - compile it with Continue instead.
$prevEap = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
if (-not ("CG" -as [type])) {
Add-Type @"
using System;
using System.Runtime.InteropServices;
public class CG {
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int c);
  [DllImport("user32.dll")] public static extern bool BringWindowToTop(IntPtr h);
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
  [DllImport("user32.dll")] public static extern void mouse_event(uint f, uint dx, uint dy, uint d, IntPtr e);
  [DllImport("user32.dll")] public static extern void keybd_event(byte vk, byte scan, uint flags, IntPtr extra);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
  [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h, IntPtr pid);
  [DllImport("kernel32.dll")] public static extern uint GetCurrentThreadId();
  [DllImport("user32.dll")] public static extern bool AttachThreadInput(uint from, uint to, bool attach);
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }
}
"@
}
$ErrorActionPreference = $prevEap

function Get-GameWindow {
  $p = Get-Process -Name DurangoV2 -ErrorAction SilentlyContinue | Select-Object -First 1
  if ($null -eq $p) { return [IntPtr]::Zero }
  return $p.MainWindowHandle
}

# Windows refuses SetForegroundWindow from a background process; the fake ALT
# press + AttachThreadInput is the standard way around it. Without this the
# clicks land on whatever window is actually on top (ask me how I know).
function Focus-Game {
  $h = Get-GameWindow
  if ($h -eq [IntPtr]::Zero) { return $false }
  for ($i = 0; $i -lt 6; $i++) {
    if ([CG]::GetForegroundWindow() -eq $h) { return $true }
    $target = [CG]::GetWindowThreadProcessId($h, [IntPtr]::Zero)
    $me = [CG]::GetCurrentThreadId()
    [CG]::AttachThreadInput($me, $target, $true) | Out-Null
    [CG]::keybd_event(0x12, 0, 0, [IntPtr]::Zero)
    [CG]::ShowWindow($h, 9) | Out-Null
    [CG]::BringWindowToTop($h) | Out-Null
    [CG]::SetForegroundWindow($h) | Out-Null
    [CG]::keybd_event(0x12, 0, 2, [IntPtr]::Zero)
    [CG]::AttachThreadInput($me, $target, $false) | Out-Null
    Start-Sleep -Milliseconds 400
  }
  return ([CG]::GetForegroundWindow() -eq $h)
}

function Click-Game([int]$x, [int]$y, [string]$what) {
  if (-not (Focus-Game)) { throw "game window is not foreground - aborting before clicking blind" }
  $r = New-Object CG+RECT
  [CG]::GetWindowRect((Get-GameWindow), [ref]$r) | Out-Null
  [CG]::SetCursorPos(($r.Left + $x), ($r.Top + $y)) | Out-Null
  Start-Sleep -Milliseconds 250
  [CG]::mouse_event(0x0002, 0, 0, 0, [IntPtr]::Zero)
  Start-Sleep -Milliseconds 80
  [CG]::mouse_event(0x0004, 0, 0, 0, [IntPtr]::Zero)
  Start-Sleep -Milliseconds 500
  Write-Output ("  clicked {0,-28} at window {1},{2}" -f $what, $x, $y)
}

# Click at a FRACTION of the window (0..1), not a fixed pixel.
#
# Why: the game ignores -screen-width/-screen-height when a previous run saved a window
# size in the registry, so the window can differ from the requested 800x458
# (which is an 816x497 window after Windows adds its 16x39 title-bar frame).
# Fixed pixel coords then miss the button, the title screen never advances, and it looks
# exactly like "the game is stuck on the loading screen" - which cost us an hour once.
function Click-GameFrac([double]$fx, [double]$fy, [string]$what) {
  if (-not (Focus-Game)) { throw "game window is not foreground - aborting before clicking blind" }
  $r = New-Object CG+RECT
  [CG]::GetWindowRect((Get-GameWindow), [ref]$r) | Out-Null
  $w = $r.Right - $r.Left; $h = $r.Bottom - $r.Top
  $x = [int]($w * $fx); $y = [int]($h * $fy)
  Click-Game $x $y "$what (${w}x${h})"
}

function Save-Shot([string]$out) {
  if (-not (Focus-Game)) { return }
  $r = New-Object CG+RECT
  [CG]::GetWindowRect((Get-GameWindow), [ref]$r) | Out-Null
  $w = $r.Right - $r.Left; $h = $r.Bottom - $r.Top
  $bmp = New-Object System.Drawing.Bitmap $w, $h
  $g = [System.Drawing.Graphics]::FromImage($bmp)
  $g.CopyFromScreen($r.Left, $r.Top, 0, 0, $bmp.Size)
  $bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
  $g.Dispose(); $bmp.Dispose()
  Write-Output "  saved $out"
}

if (-not $SkipLaunch) {
  # NEVER launch a second instance: the client runs its own island server on 8390/8391,
  # so a second copy fails to bind and the process dies (crash dumps, no useful log).
  # Always reuse the running one instead.
  $running = @(Get-Process -Name DurangoV2 -ErrorAction SilentlyContinue)
  if ($running.Count -gt 1) {
    throw "$($running.Count) game instances are running - close all but one first (they fight over ports 8390/8391)"
  }
  if ((Get-GameWindow) -ne [IntPtr]::Zero) {
    Write-Output "game already running - reusing it"
  } else {
    Write-Output "launching game (autoconnect -> $Ip)..."
    # The dll patch (PatchAutoConnect) makes Server.BeginServer read this env var and
    # call ConnectTo() by itself - that removes 5 of the 6 UI clicks, so connecting no
    # longer depends on window coordinates that drift when the UI changes.
    if ($Manual) {
      Remove-Item Env:DURANGO_AUTOCONNECT -ErrorAction SilentlyContinue
    } else {
      $env:DURANGO_AUTOCONNECT = $Ip
    }
    # Start-Process flattens an argument array without preserving the quotes around
    # a path with spaces.  Keep this one command line so Unity writes client.log
    # inside the game directory, rather than a stray file named "Desktop\\Durango".
    $clientLog = Join-Path $gameDir 'client.log'
    $launchArgs = '-force-d3d11 -screen-fullscreen 0 -screen-width 800 -screen-height 458 -logFile "' + $clientLog + '"'
    Start-Process -FilePath (Join-Path $gameDir "DurangoV2.exe") `
      -ArgumentList $launchArgs `
      -WorkingDirectory $gameDir
    # loading the island takes a while on a cold start
    for ($i = 0; $i -lt 60; $i++) {
      Start-Sleep -Seconds 2
      if ((Get-GameWindow) -ne [IntPtr]::Zero) { break }
    }
    Start-Sleep -Seconds 20
  }
}

if ((Get-GameWindow) -eq [IntPtr]::Zero) { throw "game window never appeared" }

if (-not $InGame) {
  # Title screen: server/character pickers keep last time's choice, so only Start is needed.
  # With DURANGO_AUTOCONNECT set, the client connects on its own right after the island loads
  # and the six menu clicks below are not reached at all.
  Write-Output "title screen: pressing Start, then waiting for the island to load..."
  Click-GameFrac 0.50 0.794 "Start"
  Start-Sleep -Seconds 60
  if (-not $Manual -and $env:DURANGO_AUTOCONNECT) {
    Write-Output "autoconnect should have run - check the server log for '[world] player joined'"
    return
  }
  for ($i = 0; $i -lt 6; $i++) { Click-GameFrac 0.50 0.956 "dialogue next" | Out-Null; Start-Sleep -Milliseconds 500 }
}

# fallback path: drive the menu by hand (used when autoconnect is unavailable)
Write-Output "connecting to $Ip via the menu..."
Click-GameFrac 0.05 0.94 "main menu (hamburger)"
Click-GameFrac 0.122 0.377 "visit friend's island"
Click-GameFrac 0.417 0.254 "direct input"
Click-GameFrac 0.50 0.849 "OK (server list)"
# The IP box is prefilled from Preferences("last_connect_ip"); retype only if different.
if ($Ip -ne "127.0.0.1") {
  [System.Windows.Forms.SendKeys]::SendWait("^a")
  [System.Windows.Forms.SendKeys]::SendWait($Ip)
  Start-Sleep -Milliseconds 300
}
Click-GameFrac 0.666 0.162 "OK (ip input)"
Click-GameFrac 0.50 0.557 "OK (travel confirm)"

Write-Output "loading the server world (this takes ~30s)..."
Start-Sleep -Seconds 30
# The intro/tutorial dialogue blocks input until clicked through - it is decided
# client-side (GameManager.IsPrologueMode) so the server cannot turn it off.
# In manual-IP mode, leave the loaded game untouched: clicking at the bottom while
# the login curtain is still changing state can turn a successful login into a 400.
if (-not $Manual) {
  for ($i = 0; $i -lt 8; $i++) { Click-GameFrac 0.50 0.956 "dialogue next" | Out-Null; Start-Sleep -Milliseconds 600 }
}

Write-Output "done - check the server log for '[world] player joined'"
