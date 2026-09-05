# shot.ps1 - capture the game window to shots\<tag>-<time>.png (window only, cursor untouched)
#
#   powershell -File tools\shot.ps1
#   powershell -File tools\shot.ps1 -Tag craft
#
# Finds the window via EnumWindows-by-PID rather than Process.MainWindowHandle: the
# latter goes stale (returns 0) once the game has been running a while or after certain
# window-state changes, even via .Refresh(). EnumWindows always sees the real state,
# including a window that Windows has marked not-visible (we restore it below).
#
# Captures via PrintWindow(PW_RENDERFULLCONTENT) straight from the window handle -
# no SetForegroundWindow/AttachThreadInput/keybd_event dance needed. Old CopyFromScreen
# approach required the window to be physically on top of everything on the real screen,
# which meant stealing focus from whatever the user was doing every single shot.
# PrintWindow reads the window's own render surface (works for DirectX/Unity content
# via PW_RENDERFULLCONTENT = 2 on Win10/11) regardless of z-order or focus.
#
# NOTE: ASCII only - Windows PowerShell 5.1 reads .ps1 files as ANSI.

param(
  [string]$Tag = "shot"
)

$ErrorActionPreference = "Stop"
$root    = Split-Path -Parent $PSScriptRoot
$shotDir = Join-Path $root "shots"

# DurangoV2 = ชุดเก่า, Durango = ชุดแจกจริง (dist\DurangoTH-Clean\Durango.exe) - รับทั้งสองชื่อ
$proc = @(Get-Process -Name DurangoV2 -ErrorAction SilentlyContinue)
if ($proc.Count -eq 0) { $proc = @(Get-Process -Name Durango -ErrorAction SilentlyContinue) }
if ($proc.Count -eq 0) { Write-Output "game is not running"; exit 1 }
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
  public delegate bool EnumProc(IntPtr hWnd, IntPtr lParam);
  [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc lpEnumFunc, IntPtr lParam);
  [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
  [DllImport("user32.dll")] public static extern int GetWindowTextLength(IntPtr hWnd);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
  [DllImport("user32.dll")] public static extern bool IsIconic(IntPtr h);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int c);
  [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);
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

$h = [DG]::FindMainWindow([uint32]$proc[0].Id)
if ($h -eq [IntPtr]::Zero) { Write-Output "no window handle yet"; exit 1 }

# PrintWindow can't render a minimized window's content - restore (not activate) if needed.
# SW_SHOWNOACTIVATE = 4: makes it visible without stealing focus/z-order.
if ([DG]::IsIconic($h)) { [void][DG]::ShowWindow($h, 4) }

$r = New-Object DG+RECT
if (-not [DG]::GetWindowRect($h, [ref]$r)) { Write-Output "GetWindowRect failed"; exit 1 }
$w = $r.R - $r.L; $ht = $r.B - $r.T
if ($w -le 0 -or $ht -le 0) { Write-Output "window has no size"; exit 1 }

$bmp = New-Object System.Drawing.Bitmap $w, $ht
$g = [System.Drawing.Graphics]::FromImage($bmp)
$hdc = $g.GetHdc()
$ok = [DG]::PrintWindow($h, $hdc, 2)  # PW_RENDERFULLCONTENT
$g.ReleaseHdc($hdc)
if (-not $ok) { Write-Output "warning: PrintWindow returned false - image may be blank" }

$out = Join-Path $shotDir ("{0}-{1}.png" -f $Tag, (Get-Date -Format "HHmmss"))
$bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()
Write-Output "$out  ($w x $ht)"
