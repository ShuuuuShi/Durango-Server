# build-client.ps1 — build ตัวเกมจากซอร์สใน client\ แล้วเอาไปวางในเกมให้เลย
#
#   powershell -File tools\build-client.ps1              # build + วางลงเกม
#   powershell -File tools\build-client.ps1 -Restore     # ย้อนกลับไป DLL ก่อนหน้า
#   powershell -File tools\build-client.ps1 -NoInstall   # build เฉย ๆ ไม่แตะเกม
#
# ทำไมถึงทำได้: client\ คือซอร์สที่ถอดจาก Assembly-CSharp.dll ด้วย ILSpy แล้ว **คอมไพล์ผ่าน**
# (`client\Assembly-CSharp.csproj` อ้าง DLL ของ Unity ในเกมโดยตรง) เทสแล้วว่าเกมรันได้จริง
# และต่อเข้า server ได้ — จึงไม่ต้องพึ่ง IL patch (tools\DllPatcher) อีกต่อไป แก้ซอร์สตรง ๆ ได้เลย
#
# ⚠️ ไฟล์นี้ต้องเซฟเป็น UTF-8 **มี BOM** (PowerShell 5.1 อ่านไฟล์ไม่มี BOM เป็น ANSI)

param(
  [switch]$Restore,
  [switch]$NoInstall
)

$ErrorActionPreference = 'Stop'
$root    = Split-Path -Parent $PSScriptRoot
$client  = Join-Path $root 'client'
$built   = Join-Path $client 'bin\Release\net35\Assembly-CSharp.dll'
$target  = Join-Path $root 'game\DurangoV2_Data\Managed\Assembly-CSharp.dll'
$backups = Join-Path $root 'game-backup'
# ระบบ mod ฝั่งเกม (24 ส.ค. 2026) — Assembly-CSharp.dll อ้างอิง DurangoClientModSdk.dll (ProjectReference)
# ต้องวางคู่กันใน Managed\ ด้วยเสมอ ไม่งั้น Mono resolve ไม่เจอตอนเกมรัน
$builtSdk  = Join-Path $client 'bin\Release\net35\DurangoClientModSdk.dll'
$targetSdk = Join-Path $root 'game\DurangoV2_Data\Managed\DurangoClientModSdk.dll'
# ClientMethodOverrideManager uses Harmony at runtime.  The compiler copies the
# package DLL beside Assembly-CSharp.dll, so deployment must copy it as well.
$builtHarmony  = Join-Path $client 'bin\Release\net35\0Harmony.dll'
$targetHarmony = Join-Path $root 'game\DurangoV2_Data\Managed\0Harmony.dll'

function Say($t, $c = 'Gray') { Write-Host $t -ForegroundColor $c }

function Stop-Game {
  $p = Get-Process DurangoV2 -ErrorAction SilentlyContinue
  if ($p) { Say 'ปิดเกมก่อน (DLL ถูกล็อกอยู่)...' 'Yellow'; $p | Stop-Process -Force; Start-Sleep -Seconds 3 }
}

if ($Restore) {
  $last = Get-ChildItem $backups -Filter 'Assembly-CSharp.*.dll' -ErrorAction SilentlyContinue |
          Sort-Object LastWriteTime -Descending | Select-Object -First 1
  if (-not $last) { Say 'ไม่มีไฟล์สำรองให้ย้อนกลับ' 'Red'; exit 1 }
  Stop-Game
  Copy-Item $last.FullName $target -Force
  Say "ย้อนกลับเป็น $($last.Name) แล้ว" 'Green'
  exit 0
}

Say 'build ตัวเกมจากซอร์ส (client\Assembly-CSharp.csproj)...' 'Cyan'
$out = & dotnet build $client -c Release -v q --nologo 2>&1
if ($LASTEXITCODE -ne 0) {
  Say 'build ไม่ผ่าน:' 'Red'
  $out | Select-String -Pattern ': error ' | Select-Object -First 15 | ForEach-Object { Say "  $_" 'Red' }
  exit 1
}
$size = [math]::Round((Get-Item $built).Length / 1MB, 2)
Say "build ผ่าน — $built ($size MB)" 'Green'

if ($NoInstall) { exit 0 }

Stop-Game

# สำรองของเดิมไว้เสมอ (เก็บ 10 ไฟล์ล่าสุด) — ย้อนกลับด้วย -Restore
if (-not (Test-Path $backups)) { New-Item -ItemType Directory -Path $backups | Out-Null }
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
Copy-Item $target (Join-Path $backups "Assembly-CSharp.$stamp.dll") -Force
Get-ChildItem $backups -Filter 'Assembly-CSharp.2*.dll' | Sort-Object LastWriteTime -Descending |
  Select-Object -Skip 10 | Remove-Item -Force -ErrorAction SilentlyContinue

Copy-Item $built $target -Force
if (Test-Path $builtSdk) { Copy-Item $builtSdk $targetSdk -Force }
if (Test-Path $builtHarmony) { Copy-Item $builtHarmony $targetHarmony -Force }
Say "วางลงเกมแล้ว: $target" 'Green'
Say 'เปิดเกมได้เลย (เมนูข้อ 1 หรือ 3) — ถ้าพังให้ย้อนกลับด้วย -Restore' 'DarkGray'
