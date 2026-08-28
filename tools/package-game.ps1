# package-game.ps1 — แพ็กตัวเกมเป็นชุดแจกให้คนนอกเอาไปเทส
#
#   powershell -File tools\package-game.ps1                      # ชุดที่ชี้ 127.0.0.1
#   powershell -File tools\package-game.ps1 -Ip 203.0.113.10      # ใส่ที่อยู่ VPS ให้เลย
#   powershell -File tools\package-game.ps1 -NoZip                # ทำแค่โฟลเดอร์ ไม่ต้องบีบอัด
#   powershell -File tools\package-game.ps1 -SkipBuild            # ไม่ต้อง build client ใหม่
#
# สิ่งที่สคริปต์นี้กันไว้ให้:
#   - DLL ในชุดแจก **ต้องตรงกับซอร์สใน client\** (เทียบ SHA256) ไม่งั้นหยุดทันที
#     — กันเคสแจกตัวเกมที่ลืม build ทับ แล้วคนเทสเจอบั๊กที่แก้ไปแล้ว
#   - ไม่แจก AppData/AppData2 (ตัวละคร+โลกออฟไลน์ของเครื่องคนทำ) ⇒ คนเทสเริ่มจากศูนย์
#   - ไม่แจก log/เอกสารภายใน/ไฟล์ขยะที่ค้างอยู่ในโฟลเดอร์เกม
#
# ⚠️ ไฟล์นี้ต้องเซฟเป็น UTF-8 **มี BOM** (PowerShell 5.1 อ่านไฟล์ไม่มี BOM เป็น ANSI)

param(
  [string]$Ip = '127.0.0.1',
  [string]$Out,
  [string]$Name = 'DurangoTH',
  [switch]$SkipBuild,
  [switch]$NoZip,
  # [แก้เอง] 24 ส.ค. 2026 — เวอร์ชันสำหรับระบบออโต้อัพเดท (DurangoUpdater) เทียบสตริงตรงๆ กับ
  # manifest.json บน GitHub Release ไม่ต้องเรียงเลขจริงจัง แค่ "ไม่เหมือนรอบก่อน" ก็พอ — default
  # เป็นวันที่+เวลาปัจจุบันกันลืมตั้ง
  [string]$Version = (Get-Date -Format 'yyyy-MM-dd-HHmm'),
  [string]$ManifestRepo = 'SuperCodeTH/Durango-TH-Client',
  [switch]$SkipUpdater,
  # ระบุถ้ารู้ tag ที่จะใช้ตอน `gh release create` ล่วงหน้า — สคริปต์จะเขียน zip_url ที่ถูกต้องให้เลย
  # ไม่ระบุก็ยังได้ manifest.json ออกมา แต่ต้องแก้ zip_url เองหลังอัปโหลดจริง
  [string]$ReleaseTag = ''
)

$ErrorActionPreference = 'Stop'
$root    = Split-Path -Parent $PSScriptRoot
$gameDir = Join-Path $root 'game'
$tpl     = Join-Path $PSScriptRoot 'dist-template'
if (-not $Out) { $Out = Join-Path $root 'dist' }
$stage = Join-Path $Out $Name

function Say($t, $c = 'Gray') { Write-Host $t -ForegroundColor $c }

if (-not (Test-Path $gameDir)) { throw "ไม่เจอโฟลเดอร์เกมที่ $gameDir" }
if (-not (Test-Path $tpl))     { throw "ไม่เจอ template ที่ $tpl" }

# ── 1. เกมต้องไม่เปิดอยู่ (ไฟล์ถูกล็อก + game.log จะโดนก๊อปไปด้วย) ────────────
$proc = Get-Process DurangoV2 -ErrorAction SilentlyContinue
if ($proc) {
  Say 'ปิดเกมก่อน (ไฟล์ถูกล็อกอยู่)...' 'Yellow'
  $proc | Stop-Process -Force
  Start-Sleep -Seconds 3
}

# ── 2. build ตัวเกมจากซอร์ส แล้ววางทับใน game\ ──────────────────────────────
if (-not $SkipBuild) {
  Say 'build ตัวเกมจากซอร์สก่อน (tools\build-client.ps1)...' 'Cyan'
  & (Join-Path $PSScriptRoot 'build-client.ps1')
}

# ── 3. ยืนยันว่า DLL ในเกม = ตัวที่ build ล่าสุด ────────────────────────────
$built  = Join-Path $root 'client\bin\Release\net35\Assembly-CSharp.dll'
$inGame = Join-Path $gameDir 'DurangoV2_Data\Managed\Assembly-CSharp.dll'
if (-not (Test-Path $inGame)) { throw "ไม่มี $inGame — ตัวเกมยังไม่ถูก patch" }
if (Test-Path $built) {
  $h1 = (Get-FileHash $built  -Algorithm SHA256).Hash
  $h2 = (Get-FileHash $inGame -Algorithm SHA256).Hash
  if ($h1 -ne $h2) {
    throw 'DLL ในเกมไม่ตรงกับที่ build ล่าสุด — รัน tools\build-client.ps1 (ไม่ใส่ -NoInstall) ก่อน'
  }
  Say 'DLL ในเกมตรงกับซอร์สล่าสุด (SHA256 ตรงกัน)' 'Green'
} else {
  Say 'ยังไม่เคย build client — ข้ามการเทียบ SHA256' 'Yellow'
}

# ── 4. ก๊อปเข้า staging (ตัดของที่ไม่ควรแจก) ───────────────────────────────
if (Test-Path $stage) {
  Say "ลบชุดเก่าที่ $stage ..." 'DarkGray'
  Remove-Item $stage -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $stage | Out-Null

# AppData/AppData2 = ตัวละคร+โลกออฟไลน์ของเครื่องคนทำ — แจกไปคนเทสจะได้ตัวละครเราติดไปด้วย
$excludeDirs  = @('AppData', 'AppData2')
# 127.0.0.1 = ไฟล์ขยะที่เกิดจาก echo ใน launch-autoconnect.bat รุ่นเก่า (redirect หลุด)
$excludeFiles = @('*.log', 'desktop.ini', '127.0.0.1',
                  'CODE-REVIEW.md', 'ONLINE-REVIEW.md', 'README.md',
                  'launch.bat', 'launch-autoconnect.bat')

Say 'ก๊อปตัวเกมเข้าชุดแจก (ใช้เวลาสักครู่)...' 'Cyan'
$rcArgs = @($gameDir, $stage, '/E', '/NFL', '/NDL', '/NJH', '/NJS', '/NP', '/R:1', '/W:1',
            '/XD') + $excludeDirs + @('/XF') + $excludeFiles
& robocopy @rcArgs | Out-Null
$rc = $LASTEXITCODE
$global:LASTEXITCODE = 0
if ($rc -ge 8) { throw "robocopy ล้มเหลว (exit $rc)" }

# ── 5. ไฟล์สำหรับคนเทส ─────────────────────────────────────────────────────
Copy-Item (Join-Path $tpl 'เล่นเกม.bat')      $stage -Force
Copy-Item (Join-Path $tpl 'อ่านก่อนเล่น.txt') $stage -Force

# เก็บ readme เดิมของ Kyllox ไว้ให้เครดิต (ฐานตัวรันฝั่งผู้เล่นมาจากโปรเจกต์นั้น)
$kyllox = Join-Path $gameDir 'README.md'
if (Test-Path $kyllox) { Copy-Item $kyllox (Join-Path $stage 'README-Kyllox.md') -Force }

# server.txt — คนเทสแก้ไฟล์นี้ไฟล์เดียวก็ย้ายเซิร์ฟได้ ไม่ต้องแตะ .bat
#
# ⚠️ [แก้เอง] 24 ส.ค. 2026 — ห้ามใช้ Set-Content -Encoding UTF8 ตรงนี้เด็ดขาด: PowerShell 5.1
# แปลว่า "UTF-8 พร้อม BOM" เสมอ (ต่างจาก .NET ทั่วไปที่ UTF8 เฉย ๆ ไม่มี BOM) — เล่นเกม.bat อ่านไฟล์นี้
# ด้วย `for /f` ของ cmd.exe ซึ่งไม่ strip BOM ให้ ⇒ ตัวแปร SERVER ที่ได้มีอักขระ BOM (U+FEFF) แฝงอยู่
# ⇒ DURANGO_AUTOCONNECT พังไปด้วย ⇒ ตัวเกม throw UriFormatException ("hostname ไม่ได้") ตอน KnockSystem
# เงียบ ๆ แล้วค้างอยู่หน้าไตเติ้ล — เจอจากเทสแจกจริงกับเจ้าของ (ตัว .bat เขียนด้วยมือไม่โดน เพราะไม่ผ่าน
# Set-Content เลย) ⇒ ต้องเขียนแบบ UTF8 **ไม่มี BOM** ตรง ๆ ด้วย .NET encoding object เท่านั้น
$serverLines = @(
  '# ใส่ที่อยู่เซิร์ฟที่ผู้ดูแลให้มา — บรรทัดเดียวพอ',
  '# ใส่ได้ทั้ง   1.2.3.4   หรือ   1.2.3.4:8190',
  '# บรรทัดที่ขึ้นต้นด้วย # เป็นคอมเมนต์ ตัวเกมไม่อ่าน',
  $Ip
)
$serverTxtPath = Join-Path $stage 'server.txt'
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllLines($serverTxtPath, $serverLines, $utf8NoBom)

# ── 5b. ระบบออโต้อัพเดท (DurangoUpdater.exe) ───────────────────────────────
# เล่นเกม.bat เรียกตัวนี้แทน DurangoV2.exe ตรงๆ — เช็คเวอร์ชันกับ manifest.json บน GitHub Release
# ก่อนเปิดเกมทุกครั้ง ดู tools/Updater/Program.cs สำหรับ logic เต็ม
$manifestJsonPath = $null
if (-not $SkipUpdater) {
  Say 'build ตัวอัปเดต (tools\Updater)...' 'Cyan'
  $updaterProj = Join-Path $root 'tools\Updater\DurangoUpdater.csproj'
  & dotnet publish $updaterProj -c Release -r win-x64 -p:SelfContained=true --nologo -v quiet
  if ($LASTEXITCODE -ne 0) { throw "build DurangoUpdater ล้มเหลว (exit $LASTEXITCODE)" }
  $updaterExe = Join-Path $root 'tools\Updater\bin\Release\net9.0-windows\win-x64\publish\DurangoUpdater.exe'
  if (-not (Test-Path $updaterExe)) { throw "ไม่เจอ $updaterExe หลัง publish" }
  Copy-Item $updaterExe $stage -Force

  # version.txt — DurangoUpdater เทียบค่านี้กับ manifest.json ทุกครั้งที่เปิดเกม
  [System.IO.File]::WriteAllText((Join-Path $stage 'version.txt'), $Version, $utf8NoBom)

  # update-manifest-url.txt — แก้ไฟล์นี้ได้ถ้าจะย้ายไปเช็คอัปเดตจากที่อื่น (ไม่ต้อง build ใหม่)
  $manifestUrl = "https://github.com/$ManifestRepo/releases/latest/download/manifest.json"
  $manifestUrlLines = @(
    '# URL ของ manifest.json ที่ DurangoUpdater เช็คเวอร์ชันด้วย — บรรทัดเดียวพอ',
    '# ปกติไม่ต้องแก้ไฟล์นี้',
    $manifestUrl
  )
  [System.IO.File]::WriteAllLines((Join-Path $stage 'update-manifest-url.txt'), $manifestUrlLines, $utf8NoBom)

  Say ("ตัวอัปเดตพร้อม — เวอร์ชันชุดนี้: {0}" -f $Version) 'Green'
}

# ── 6. สรุป ────────────────────────────────────────────────────────────────
$files = Get-ChildItem $stage -Recurse -File
$size  = ($files | Measure-Object -Property Length -Sum).Sum
Say ''
Say ("ชุดแจกอยู่ที่ : {0}" -f $stage) 'Green'
Say ("ขนาด          : {0:N2} GB ({1:N0} ไฟล์)" -f ($size / 1GB), $files.Count) 'Gray'
Say ("เซิร์ฟตั้งต้น  : {0}" -f $Ip) 'Gray'

if ($NoZip) {
  Say 'ข้ามการบีบอัด (-NoZip)' 'DarkGray'
  return
}

# ── 7. บีบอัด ──────────────────────────────────────────────────────────────
# ใช้ .NET ตรง ๆ แทน Compress-Archive — เร็วกว่ามากและไม่อมแรมทั้งก้อน
$zip = Join-Path $Out ($Name + '.zip')
if (Test-Path $zip) { Remove-Item $zip -Force }
Say ''
Say 'กำลังบีบอัด — ชุดนี้ใหญ่ ใช้เวลาหลายนาที...' 'Yellow'
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory(
  $stage, $zip, [System.IO.Compression.CompressionLevel]::Optimal, $true)

$zipSize = (Get-Item $zip).Length
$hash    = (Get-FileHash $zip -Algorithm SHA256).Hash
Say ''
Say ("ไฟล์แจก : {0}" -f $zip) 'Green'
Say ("ขนาด    : {0:N2} GB" -f ($zipSize / 1GB)) 'Gray'
Say ("SHA256  : {0}" -f $hash) 'DarkGray'

# ── 8. manifest.json — อัปโหลดเป็น asset คู่กับ zip บน GitHub Release เดียวกัน ─────
# DurangoUpdater (ของผู้เล่นที่มีอยู่แล้ว) เช็คไฟล์นี้ทุกครั้งที่เปิดเกม เทียบกับ version.txt ของตัวเอง
if (-not $SkipUpdater) {
  if ($ReleaseTag) {
    $zipUrl = "https://github.com/$ManifestRepo/releases/download/$ReleaseTag/$Name.zip"
  } else {
    $zipUrl = "TODO: ใส่ URL จริงหลังอัปโหลด เช่น https://github.com/$ManifestRepo/releases/download/<tag>/$Name.zip"
  }
  $manifest = [ordered]@{
    Version = $Version
    ZipUrl  = $zipUrl
    Sha256  = $hash.ToLower()
    Notes   = ''
  }
  $manifestPath = Join-Path $Out 'manifest.json'
  # ⚠️ เจอมาแล้วว่า Out-File -Encoding utf8 ใส่ BOM เสมอใน PowerShell 5.1 (บั๊กเดียวกับ server.txt
  # ก่อนหน้านี้) — DurangoUpdater เทสแล้วว่า System.Text.Json ยังอ่าน BOM ผ่านได้ (ไม่พังเหมือน cmd.exe
  # for /f ที่พังกับ server.txt) แต่เขียนแบบไม่มี BOM ไว้เลยดีกว่า กันปัญหากับเครื่องมืออื่นที่อาจอ่านไฟล์นี้
  $manifestJson = $manifest | ConvertTo-Json
  [System.IO.File]::WriteAllText($manifestPath, $manifestJson, $utf8NoBom)
  Say ''
  Say ("manifest.json : {0}" -f $manifestPath) 'Green'
  if (-not $ReleaseTag) {
    Say '⚠️ ไม่ได้ระบุ -ReleaseTag — ต้องแก้ ZipUrl ใน manifest.json เองก่อนอัปโหลด' 'Yellow'
  }
  Say 'อัปโหลด manifest.json เป็น asset คู่กับ zip บน release เดียวกัน (gh release upload)' 'Gray'
}

Say ''
Say 'เอา zip ไปให้คนเทสได้เลย — เขาแตกไฟล์แล้วดับเบิลคลิก เล่นเกม.bat' 'Cyan'
