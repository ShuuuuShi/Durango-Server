# click.ps1 - click inside the game window at a fraction of its size, then screenshot
#
#   powershell -File tools\click.ps1 -X 0.82 -Y 0.51            # click, no shot
#   powershell -File tools\click.ps1 -X 0.82 -Y 0.51 -Tag rope  # click then capture
#   powershell -File tools\click.ps1 -Tag only                  # capture only (no click)
#
# Fractions, not pixels: the window comes out at whatever size the registry remembers,
# so fixed pixel coords miss buttons (this bit us before - see HANDOFF).
#
# Finds the window via EnumWindows-by-PID rather than Process.MainWindowHandle: the
# latter goes stale (returns 0) once the game has been running a while, even after
# .Refresh(). See shot.ps1 for the same fix.
#
# TRIED background clicks (PostMessage WM_LBUTTONDOWN/UP straight to the window handle,
# no focus needed) on 25 Aug 2026 - the game never registered them (Return and Revive
# button stayed up after a PostMessage click landed right on it). Durango's Unity build
# apparently polls real cursor position / raw input rather than reading the Win32 message
# queue for clicks, so this game specifically needs the real OS cursor to move. Reverted
# to SetCursorPos + mouse_event (real synthetic input), which is confirmed working -
# this does require the window on top, hence the foreground dance below. If retrying
# background clicks on a *different* window someday, PostMessage is worth another look;
# it just doesn't work for this particular game.
#
# NOTE: ASCII only - Windows PowerShell 5.1 reads .ps1 files as ANSI.

param(
  [double]$X = -1,
  [double]$Y = -1,
  [string]$Tag = "",
  [int]$WaitMs = 900
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

Add-Type -AssemblyName System.Windows.Forms, System.Drawing
$prevEap = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
if (-not ("CK" -as [type])) {
  Add-Type @"
using System;
using System.Runtime.InteropServices;
public class CK {
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int L, T, R, B; }
  public delegate bool EnumProc(IntPtr hWnd, IntPtr lParam);
  [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc lpEnumFunc, IntPtr lParam);
  [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
  [DllImport("user32.dll")] public static extern int GetWindowTextLength(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
  [DllImport("user32.dll")] public static extern void mouse_event(uint f, uint dx, uint dy, uint d, IntPtr e);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int c);
  [DllImport("user32.dll")] public static extern bool BringWindowToTop(IntPtr h);
  [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
  [DllImport("kernel32.dll")] public static extern uint GetCurrentThreadId();
  [DllImport("user32.dll")] public static extern bool AttachThreadInput(uint from, uint to, bool attach);
  [DllImport("user32.dll")] public static extern void keybd_event(byte vk, byte scan, uint flags, IntPtr extra);
  public static IntPtr FindMainWindow(uint pid) {
    IntPtr found = IntPtr.Zero;
    EnumWindows((h, l) => {
      uint wpid; GetWindowThreadProcessId(h, out wpid);
      if (wpid == pid && GetWindowTextLength(h) > 0) { found = h; return false; }
      return true;
    }, IntPtr.Zero);
    return found;
  }
}
"@
}
$ErrorActionPreference = $prevEap

$proc = @(Get-Process -Name DurangoV2 -ErrorAction SilentlyContinue)
if ($proc.Count -eq 0) { Write-Output "game is not running"; exit 1 }
$h = [CK]::FindMainWindow([uint32]$proc[0].Id)
if ($h -eq [IntPtr]::Zero) { Write-Output "no window handle"; exit 1 }

if ($X -ge 0 -and $Y -ge 0) {
  # fake ALT + AttachThreadInput: Windows refuses SetForegroundWindow from a background
  # process otherwise, and clicks land on whatever window is actually on top.
  [void][CK]::ShowWindow($h, 9)
  for ($i = 0; $i -lt 6; $i++) {
    if ([CK]::GetForegroundWindow() -eq $h) { break }
    $target = 0
    [void][CK]::GetWindowThreadProcessId($h, [ref]$target)
    $me = [CK]::GetCurrentThreadId()
    [CK]::AttachThreadInput($me, $target, $true) | Out-Null
    [CK]::keybd_event(0x12, 0, 0, [IntPtr]::Zero)
    [CK]::ShowWindow($h, 5) | Out-Null
    [CK]::BringWindowToTop($h) | Out-Null
    [CK]::SetForegroundWindow($h) | Out-Null
    [CK]::keybd_event(0x12, 0, 2, [IntPtr]::Zero)
    [CK]::AttachThreadInput($me, $target, $false) | Out-Null
    Start-Sleep -Milliseconds 300
  }

  $r = New-Object CK+RECT
  [void][CK]::GetWindowRect($h, [ref]$r)
  $w = $r.R - $r.L; $ht = $r.B - $r.T

  $px = [int]($r.L + $w * $X)
  $py = [int]($r.T + $ht * $Y)
  [void][CK]::SetCursorPos($px, $py)
  Start-Sleep -Milliseconds 120
  [CK]::mouse_event(0x0002, 0, 0, 0, [IntPtr]::Zero)   # left down
  Start-Sleep -Milliseconds 60
  [CK]::mouse_event(0x0004, 0, 0, 0, [IntPtr]::Zero)   # left up
  Write-Output ("clicked {0},{1}  (window {2}x{3})" -f $px, $py, $w, $ht)
  Start-Sleep -Milliseconds $WaitMs
}

if ($Tag -ne "") {
  & (Join-Path $PSScriptRoot "shot.ps1") -Tag $Tag
}
