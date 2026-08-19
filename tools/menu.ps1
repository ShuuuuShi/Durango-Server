# menu.ps1 — เมนูเทส + กล่องเครื่องมือ Durango Claude (เรียกจาก "เทสเกม.bat")
#
# ทำไมต้องมีไฟล์นี้: ขั้นตอนเทสจริงมีกับดักที่ลืมทีไรเสียเวลาทุกที
#   1. ต้อง kill DurangoServer.exe ก่อน build ไม่งั้นไฟล์ล็อก (MSB3021)
#   2. ห้ามเปิดเซิร์ฟซ้อน 2 ตัว และห้ามเปิดเกมซ้อน 2 ตัว
#   3. --gp-check และกล่องเครื่องมือต้องเปิดเซิร์ฟด้วย --enable-cheat --admin gm
# เมนูนี้จัดการให้หมดแล้ว
#
# ⚠️ ไฟล์นี้ต้องเซฟเป็น UTF-8 **มี BOM** เท่านั้น — PowerShell 5.1 อ่านไฟล์ที่ไม่มี BOM
#    เป็น ANSI ทำให้ภาษาไทยกลายเป็นขยะทั้งไฟล์ (ส่วน .bat ต้องเป็น ASCII ล้วน)

$ErrorActionPreference = 'Continue'

$root   = Split-Path -Parent $PSScriptRoot
$server = Join-Path $root 'server'
$tester = Join-Path $root 'test-client'
$game   = Join-Path $root 'game'
$saves  = Join-Path $server 'saves'
$backups    = Join-Path $root 'saves-backup'
$whitelist  = Join-Path $server 'data\whitelist.txt'
$connectPs1 = Join-Path $PSScriptRoot 'connect-game.ps1'
$gmTargetFile = Join-Path $PSScriptRoot 'gm-target.txt'

$GamePort    = 8191
$GatewayPort = 8190
$GmBot       = 'gm'          # ชื่อบอทที่ใช้ส่งคำสั่ง control (ต้องตรงกับ --admin ตอนเปิดเซิร์ฟ)

# สัตว์ 10 ชนิดของเกาะเริ่มต้น (ดู server/ServerCore/SpawnTable.cs)
$Species = @(
    @{ id = 2042; name = 'กิ้งก่า (lv1-3, หนีอย่างเดียว)' },
    @{ id = 2015; name = 'คอมป์โซกนาทัส (lv1-4, หนี)' },
    @{ id = 2033; name = 'โดโดฟิซิส (lv2-5, หนี)' },
    @{ id = 2006; name = 'เฟนาโคดัส (lv3-6, หนี)' },
    @{ id = 2017; name = 'โปรโตเซราท็อปส์ (lv3-7, สู้กลับ)' },
    @{ id = 2009; name = 'พาราซอโรโลฟัส (lv5-9, สู้กลับ)' },
    @{ id = 2000; name = 'สเตโกซอรัส (lv6-10, หนี)' },
    @{ id = 2003; name = 'ทริเซราท็อปส์ (lv6-10, สู้กลับ)' },
    @{ id = 2002; name = 'โอวิแรปเตอร์ (lv4-8, ไล่กัดก่อน)' },
    @{ id = 2001; name = 'แร็ปเตอร์ (lv7-10, ไล่กัดก่อน)' }
)

# ───────────────────────── เครื่องมือย่อย ─────────────────────────

function Say($text, $color = 'Gray') { Write-Host $text -ForegroundColor $color }

function Test-Port([int]$port) {
    $c = New-Object System.Net.Sockets.TcpClient
    try { $c.Connect('127.0.0.1', $port); $c.Close(); return $true }
    catch { return $false }
}

function Get-ServerProc { Get-Process DurangoServer -ErrorAction SilentlyContinue }
function Get-GameProc   { Get-Process DurangoV2 -ErrorAction SilentlyContinue }

# เซิร์ฟที่เปิดค้างอยู่ เปิดมาด้วยโหมดไหน (ดูจาก command line ของ process)
function Get-ServerArgs {
    $p = Get-ServerProc
    if (-not $p) { return $null }
    $ci = Get-CimInstance Win32_Process -Filter "ProcessId=$($p.Id)" -ErrorAction SilentlyContinue
    if ($ci) { return $ci.CommandLine } else { return '' }
}

function Stop-Server {
    $p = Get-ServerProc
    if (-not $p) { Say 'เซิร์ฟไม่ได้เปิดอยู่' ; return }
    $p | Stop-Process -Force
    Start-Sleep -Seconds 2
    Say 'ปิดเซิร์ฟแล้ว' 'Yellow'
}

function Invoke-Build {
    # กับดักข้อ 1: exe ถูกล็อกถ้าเซิร์ฟยังรันอยู่
    if (Get-ServerProc) { Say 'ปิดเซิร์ฟตัวเก่าก่อน build...' 'Yellow'; Stop-Server }
    foreach ($proj in @(@{d=$server; n='server'}, @{d=$tester; n='test-client'})) {
        Say "build $($proj.n)..." 'DarkGray'
        $out = & dotnet build $proj.d -v q --nologo 2>&1
        if ($LASTEXITCODE -ne 0) {
            Say "build $($proj.n) ไม่ผ่าน:" 'Red'
            $out | Select-String -Pattern ' error ' | Select-Object -First 10 | ForEach-Object { Say "  $_" 'Red' }
            return $false
        }
    }
    Say 'build ผ่านทั้งสองตัว' 'Green'
    return $true
}

function Wait-Server([int]$seconds = 90) {
    Say 'รอเซิร์ฟเปิดพอร์ต...' 'DarkGray'
    for ($i = 0; $i -lt $seconds; $i++) {
        if (Test-Port $GamePort) { Say "เซิร์ฟพร้อมแล้ว (พอร์ต $GamePort)" 'Green'; return $true }
        Start-Sleep -Seconds 1
    }
    Say 'เซิร์ฟไม่ขึ้นภายในเวลาที่รอ — ดูหน้าต่างเซิร์ฟว่ามี error อะไร' 'Red'
    return $false
}

# เปิดเซิร์ฟใน "หน้าต่างของตัวเอง" จะได้เห็น log วิ่งตอนเล่น
function Start-Server([string[]]$serverArgs, [string]$label) {
    if (Get-ServerProc) {
        Say 'เซิร์ฟเปิดอยู่แล้ว — ใช้ตัวเดิมต่อ (ข้อ 8 = ปิดเซิร์ฟ ถ้าอยากเปิดใหม่)' 'Yellow'
        return $true
    }
    if (-not (Invoke-Build)) { return $false }
    Say "เปิดเซิร์ฟ: $label" 'Cyan'
    Start-Process -FilePath 'dotnet' -ArgumentList (@('run','--no-build','--') + $serverArgs) `
        -WorkingDirectory $server -WindowStyle Normal | Out-Null
    return (Wait-Server)
}

function Start-TestServer {
    # เทสเองต้องมี cheat (spawn สัตว์ / kill animal / die / rest) และ gp-check ก็ต้องใช้
    # --admin gm = ให้บอทชื่อ gm สั่ง control ตัวละครในเกมได้ (กล่องเครื่องมือ)
    # กับดัก: ถ้าเผลอเปิดโหมดเปิดจริง (ข้อ 9) ค้างไว้ แล้วมากดเทส จะตกยกแผงเพราะ cheat ถูกปฏิเสธ
    $running = Get-ServerArgs
    if ($null -ne $running -and $running -notmatch '--enable-cheat') {
        Say 'เซิร์ฟที่เปิดอยู่เป็นโหมดเปิดจริง (cheat ปิด) ซึ่งเทสไม่ได้ — ปิดแล้วเปิดใหม่ให้' 'Yellow'
        Stop-Server
    }
    return (Start-Server @('--enable-cheat','--admin',$GmBot) 'โหมดเทส (cheat เปิด, admin gm)')
}

function Invoke-Tester([string[]]$testerArgs, [string]$label) {
    Say "=== $label ===" 'Cyan'
    Push-Location $tester
    & dotnet run --no-build -- @testerArgs
    $code = $LASTEXITCODE
    Pop-Location
    if ($code -eq 0) { Say 'ผ่านครบ' 'Green' } else { Say "มีข้อที่ตก (exit $code)" 'Red' }
}

function Start-Game {
    $g = @(Get-GameProc)
    if ($g.Count -gt 1) {
        Say "เกมเปิดอยู่ $($g.Count) ตัว — ปิดให้เหลือตัวเดียวก่อน (มันแย่งพอร์ต 8390/8391 กันเอง)" 'Red'
        return
    }
    if (-not (Test-Port $GamePort)) {
        Say 'เซิร์ฟยังไม่เปิด — เปิดเซิร์ฟก่อน (ข้อ 1 หรือ 2)' 'Red'
        return
    }
    Say 'เปิดเกม + ต่อ 127.0.0.1 อัตโนมัติ (ใช้เวลาโหลดเกาะสักพัก)' 'Cyan'
    Say 'ถ้าสคริปต์กดปุ่ม "เริ่ม" ไม่โดน ให้กดเองที่หน้าจอไตเติ้ล — ที่เหลือ client ต่อเซิร์ฟให้เอง' 'DarkGray'
    Say 'ดูว่าต่อติดไหมจากหน้าต่างเซิร์ฟ: ต้องมีบรรทัด [world] player joined' 'DarkGray'
    & $connectPs1 -Ip 127.0.0.1
}

# ───────────────────────── กล่องเครื่องมือตอนเล่น (GM) ─────────────────────────
#
# ส่งคำสั่งเข้าไปหาตัวละคร "ที่ล็อกอินอยู่ในตัวเกมจริง" ผ่านบอท gm
# (แต่ละคำสั่ง = ล็อกอินบอท 1 รอบ ใช้เวลา ~5 วินาที ถือว่าคุ้มกว่าเดินหาสัตว์เอง)

function Invoke-Bot([string]$botCmd) {
    Push-Location $tester
    & dotnet run --no-build -- --console 127.0.0.1 $GamePort $GmBot --cmd $botCmd 2>&1 |
        Where-Object { $_ -notmatch '^\[(session|เข้าเกม|ย้ายตำแหน่ง)' } |
        ForEach-Object { Say "  $_" }
    Pop-Location
}

function Show-Who {
    if (-not (Test-Port $GamePort)) { Say 'เซิร์ฟยังไม่เปิด' 'Red'; return }
    Say 'ใครออนไลน์อยู่บ้าง:' 'Cyan'
    Invoke-Bot 'cheat who'
}

function Get-GmTarget {
    if (Test-Path $gmTargetFile) {
        $t = (Get-Content $gmTargetFile -Raw -Encoding UTF8).Trim()
        if ($t) { return $t }
    }
    Say 'ยังไม่ได้ตั้งว่าจะสั่งตัวละครไหน — ดูรายชื่อที่ออนไลน์ก่อน' 'Yellow'
    Show-Who
    $t = (Read-Host '  พิมพ์ชื่อตัวละครในเกม (หรือ entity id)').Trim()
    if (-not $t) { Say 'ไม่ได้ใส่ชื่อ — ยกเลิก' 'Red'; return $null }
    Set-Content -Path $gmTargetFile -Value $t -Encoding UTF8
    return $t
}

function Invoke-Gm([string]$verb) {
    if (-not (Test-Port $GamePort)) { Say 'เซิร์ฟยังไม่เปิด' 'Red'; return }
    $target = Get-GmTarget
    if (-not $target) { return }
    Say "ส่งคำสั่ง: control $target $verb" 'DarkGray'
    Invoke-Bot "control $target $verb"
}

function Show-GmMenu {
    $target = if (Test-Path $gmTargetFile) { (Get-Content $gmTargetFile -Raw -Encoding UTF8).Trim() } else { '(ยังไม่ได้ตั้ง)' }
    Say ''
    Say '  ┌── กล่องเครื่องมือตอนเล่น ──────────────────────┐' 'Cyan'
    Say "  │ สั่งตัวละคร: $target" 'Cyan'
    Say '  └────────────────────────────────────────────────┘' 'Cyan'
    Say '   1  เสกสัตว์มาข้างตัว (สุ่มชนิด)'
    Say '   2  เสกสัตว์มาข้างตัว (เลือกชนิด)'
    Say '   3  ฆ่าสัตว์ตัวใกล้ที่สุด  → ได้ซากไว้เทสแล่เนื้อ'
    Say '   4  เติมเลือด/สตามินา (ตายอยู่ = ฟื้นให้)'
    Say '   5  เสกของ (ขวาน/กองไฟ/กล่อง/ชุด)'
    Say '   6  วาร์ปกลับจุดเกิด (tile 40,177)'
    Say '   7  ดูสถานะตัวละคร'
    Say '   8  เก็บของธรรมชาติที่ใกล้ที่สุดให้'
    Say '   9  เปลี่ยนตัวละครที่จะสั่ง / ดูใครออนไลน์'
    Say '   0  กลับเมนูหลัก'
    Say ''
}

function Enter-GmToolbox {
    if (-not (Test-Port $GamePort)) { Say 'เซิร์ฟยังไม่เปิด — เปิดก่อน (ข้อ 2)' 'Red'; return }
    while ($true) {
        Show-GmMenu
        $pick = (Read-Host '  เลือก').Trim()
        Say ''
        switch ($pick) {
            '1' { Invoke-Gm 'spawn' }
            '2' {
                for ($i = 0; $i -lt $Species.Count; $i++) { Say ("   {0,2}  {1}" -f ($i + 1), $Species[$i].name) }
                $n = (Read-Host '  เลือกชนิด (1-10)').Trim()
                if ($n -match '^\d+$' -and [int]$n -ge 1 -and [int]$n -le $Species.Count) {
                    Invoke-Gm ('spawn ' + $Species[[int]$n - 1].id)
                } else { Say 'ไม่มีข้อนี้' 'Red' }
            }
            '3' { Invoke-Gm 'kill' }
            '4' { Invoke-Gm 'heal' }
            '5' {
                Say '   1 ขวานหิน   2 กองไฟ   3 กล่องใบไม้   4 ชุดช่าง'
                Say '   5 ชุดทำอาหาร (เนื้อ/กิ่งไม้/น้ำ/หม้อ/เตาย่าง/กองไฟ+กองไฟใหญ่)'
                Say '   6 พิมพ์ชื่อ prototype เอง (เช่น meat, stone, pot_02)'
                $n = (Read-Host '  เลือก').Trim()
                $what = switch ($n) { '1' { 'axe' } '2' { 'bonfire' } '3' { 'box' } '4' { 'clothes' } '5' { 'cook' } '6' { (Read-Host '  ชื่อ prototype').Trim() } default { $null } }
                if ($what) { Invoke-Gm "give $what" } else { Say 'ไม่มีข้อนี้' 'Red' }
            }
            '6' { Invoke-Gm 'tp 40 177' }
            '7' { Invoke-Gm 'status' }
            '8' { Invoke-Gm 'gather' }
            '9' {
                if (Test-Path $gmTargetFile) { Remove-Item $gmTargetFile -Force }
                Get-GmTarget | Out-Null
            }
            '0' { return }
            default { Say 'ไม่มีข้อนี้' 'Red' }
        }
        Say ''
        Read-Host '  กด Enter เพื่อไปต่อ' | Out-Null
    }
}

# ───────────────────────── ตั้งค่าเซิร์ฟ (data/config.json) ─────────────────────────
#
# ค่าปรับสมดุลทั้งหมดอยู่ในไฟล์ JSON ไฟล์เดียว เซิร์ฟอ่านซ้ำทุก 5 วินาที
# แก้แล้วมีผลทันทีโดยไม่ต้อง build/รีสตาร์ท (ยกเว้นตารางสัตว์ ที่ต้องเปิดเซิร์ฟใหม่)

$configFile = Join-Path $server 'data\config.json'

function Read-Config {
    if (-not (Test-Path $configFile)) {
        Say 'ยังไม่มี data/config.json — เปิดเซิร์ฟสักครั้ง (ข้อ 2) เดี๋ยวมันสร้างให้เอง' 'Yellow'
        return $null
    }
    try { return (Get-Content $configFile -Raw -Encoding UTF8 | ConvertFrom-Json) }
    catch { Say "ไฟล์ config เสีย: $_" 'Red'; return $null }
}

function Write-Config($cfg) {
    ($cfg | ConvertTo-Json -Depth 8) | Set-Content $configFile -Encoding UTF8
    Say 'บันทึกแล้ว — เซิร์ฟที่เปิดอยู่จะรับค่าใหม่ภายใน 5 วินาที' 'Green'
    Say '(ยกเว้นตารางสัตว์: ชนิด/โควตา มีผลตอนเปิดเซิร์ฟใหม่ เพราะสัตว์เกิดไปแล้วตั้งแต่ตอนเปิด)' 'DarkGray'
}

# แก้ค่าตัวเลขทีละตัวจากรายการที่ยกมาให้เลือก
function Edit-Numbers($node, $fields, $title) {
    while ($true) {
        Say ''
        Say "  $title" 'Cyan'
        for ($i = 0; $i -lt $fields.Count; $i++) {
            $f = $fields[$i]
            Say ("   {0,2}  {1,-22} = {2}   {3}" -f ($i + 1), $f.key, $node.($f.key), $f.note)
        }
        Say '    0  กลับ'
        $p = (Read-Host '  เลือกข้อที่จะแก้').Trim()
        if ($p -eq '0' -or $p -eq '') { return $false }
        if ($p -notmatch '^\d+$' -or [int]$p -lt 1 -or [int]$p -gt $fields.Count) { Say 'ไม่มีข้อนี้' 'Red'; continue }
        $f = $fields[[int]$p - 1]
        $v = (Read-Host "  $($f.key) ใหม่ (ตอนนี้ $($node.($f.key)))").Trim()
        if ($v -eq '') { continue }
        $num = 0.0
        if (-not [double]::TryParse($v, [ref]$num)) { Say 'ต้องเป็นตัวเลข' 'Red'; continue }
        if ($num -lt 0) { Say 'ติดลบไม่ได้' 'Red'; continue }
        $node.($f.key) = $num
        return $true
    }
}

function Edit-SpawnTable($cfg) {
    while ($true) {
        Say ''
        Say '  ตารางสัตว์ — เรทเกิดและเลเวลของแต่ละชนิด' 'Cyan'
        Say ("   {0,2}  {1,-20} {2,-8} {3,-6} {4,-12} {5}" -f '#', 'ชนิด', 'เลเวล', 'จำนวน', 'นิสัย', 'ห่างจุดเกิด')
        for ($i = 0; $i -lt $cfg.Spawn.Count; $i++) {
            $s = $cfg.Spawn[$i]
            Say ("   {0,2}  {1,-20} {2,-8} {3,-6} {4,-12} {5} tile" -f ($i + 1), $s.Name,
                 "$($s.MinLevel)-$($s.MaxLevel)", $s.Quota, $s.Behavior, $s.MinTilesFromEntry)
        }
        Say ("   รวมทั้งเกาะ {0} ตัว" -f (($cfg.Spawn | Measure-Object -Property Quota -Sum).Sum)) 'DarkGray'
        Say '    0  กลับ'
        $p = (Read-Host '  เลือกชนิดที่จะแก้').Trim()
        if ($p -eq '0' -or $p -eq '') { return $false }
        if ($p -notmatch '^\d+$' -or [int]$p -lt 1 -or [int]$p -gt $cfg.Spawn.Count) { Say 'ไม่มีข้อนี้' 'Red'; continue }
        $s = $cfg.Spawn[[int]$p - 1]
        Say ''
        Say "  แก้ $($s.Name) (type $($s.Type))" 'Cyan'
        Say '   1 จำนวนในโลก   2 เลเวลต่ำสุด   3 เลเวลสูงสุด   4 นิสัย   5 ระยะห่างจุดเกิด   0 กลับ'
        $w = (Read-Host '  เลือก').Trim()
        switch ($w) {
            '1' { $v = (Read-Host "  จำนวน (ตอนนี้ $($s.Quota), 0 = ไม่ให้เกิดเลย)").Trim()
                  if ($v -match '^\d+$' -and [int]$v -le 200) { $s.Quota = [int]$v; return $true } else { Say 'ต้องเป็น 0-200' 'Red' } }
            '2' { $v = (Read-Host "  เลเวลต่ำสุด (ตอนนี้ $($s.MinLevel))").Trim()
                  if ($v -match '^\d+$' -and [int]$v -ge 1 -and [int]$v -le $s.MaxLevel) { $s.MinLevel = [int]$v; return $true } else { Say "ต้องเป็น 1-$($s.MaxLevel)" 'Red' } }
            '3' { $v = (Read-Host "  เลเวลสูงสุด (ตอนนี้ $($s.MaxLevel))").Trim()
                  if ($v -match '^\d+$' -and [int]$v -ge $s.MinLevel -and [int]$v -le 60) { $s.MaxLevel = [int]$v; return $true } else { Say "ต้องเป็น $($s.MinLevel)-60" 'Red' } }
            '4' { Say '   1 Flee (หนีอย่างเดียว)   2 FightBack (สู้กลับเมื่อโดนตี)   3 Aggressive (ไล่กัดก่อน)'
                  $b = (Read-Host '  เลือก').Trim()
                  $name = switch ($b) { '1' { 'Flee' } '2' { 'FightBack' } '3' { 'Aggressive' } default { $null } }
                  if ($name) { $s.Behavior = $name; return $true } else { Say 'ไม่มีข้อนี้' 'Red' } }
            '5' { $v = (Read-Host "  ต้องเกิดห่างจุดเกิดกี่ tile (ตอนนี้ $($s.MinTilesFromEntry))").Trim()
                  if ($v -match '^\d+$' -and [int]$v -le 100) { $s.MinTilesFromEntry = [int]$v; return $true } else { Say 'ต้องเป็น 0-100' 'Red' } }
            default { }
        }
    }
}

function Enter-ConfigEditor {
    while ($true) {
        $cfg = Read-Config
        if ($null -eq $cfg) { return }
        Say ''
        Say '  ┌── ตั้งค่าเซิร์ฟ (data/config.json) ────────────┐' 'Cyan'
        Say '  │ แก้แล้วมีผลทันที ไม่ต้อง build ไม่ต้องรีสตาร์ท │' 'Cyan'
        Say '  └────────────────────────────────────────────────┘' 'Cyan'
        Say '   1  เรทเกิดสัตว์ · จำนวน · เลเวล · นิสัย (รายชนิด)'
        Say '   2  สมดุลสัตว์ (เลือด · ดาเมจ · ความเร็ว · ระยะไล่)'
        Say '   3  เวลาซาก · เวลาเกิดใหม่ · รัศมีการกระจาย'
        Say '   4  เรท exp (ล่าสัตว์ · เก็บของ · คราฟต์ · แต้มสกิล)'
        Say '   7  ผลของสกิล (เก็บของเร็วขึ้น · ดาเมจ · ป้องกัน · คราฟต์)'
        Say '   5  เปิดไฟล์ด้วย Notepad (แก้เองทั้งไฟล์)'
        Say '   6  คืนค่าเริ่มต้นทั้งหมด'
        Say '   0  กลับเมนูหลัก'
        Say ''
        $pick = (Read-Host '  เลือก').Trim()
        switch ($pick) {
            '1' { if (Edit-SpawnTable $cfg) { Write-Config $cfg } }
            '2' {
                $changed = Edit-Numbers $cfg.Animals @(
                    @{key='LifeBase';      note='เลือดสัตว์ = ค่านี้ + เลเวล x LifePerLevel'},
                    @{key='LifePerLevel';  note=''},
                    @{key='DamageBase';    note='ดาเมจสัตว์ = ค่านี้ + เลเวล x DamagePerLevel'},
                    @{key='DamagePerLevel';note=''},
                    @{key='ChaseSpeed';    note='ความเร็วตอนไล่ (หน่วยโลก/วินาที)'},
                    @{key='FleeSpeed';     note='ความเร็วตอนหนี'},
                    @{key='SightTiles';    note='ตัวดุเห็นคนในระยะกี่ tile แล้วเริ่มไล่'},
                    @{key='GiveUpTiles';   note='ไล่เกินกี่ tile แล้วเลิกสนใจ'},
                    @{key='AggroSeconds';  note='โกรธนานกี่วินาที'},
                    @{key='FirstAttackDelay'; note='โดนตีแล้วกี่วินาทีถึงสวนกลับครั้งแรก'}
                ) 'สมดุลสัตว์'
                if ($changed) { Write-Config $cfg }
            }
            '3' {
                $changed = Edit-Numbers $cfg.Animals @(
                    @{key='CorpseSeconds';     note='ซากอยู่ในโลกกี่วินาทีก่อนหาย'},
                    @{key='RespawnSeconds';    note='ตายแล้วกี่วินาทีเกิดตัวใหม่'},
                    @{key='SpawnRadiusTiles';  note='กระจายจุดเกิดรอบจุดเข้าเกมกี่ tile'},
                    @{key='WanderRadiusTiles'; note='เดินออกจากบ้านตัวเองได้ไกลกี่ tile'}
                ) 'เวลาและระยะ'
                if ($changed) { Write-Config $cfg }
            }
            '4' {
                $changed = Edit-Numbers $cfg.Exp @(
                    @{key='KillBase';     note='ฆ่าสัตว์ได้ = ค่านี้ + เลเวลสัตว์ x KillPerLevel'},
                    @{key='KillPerLevel'; note=''},
                    @{key='Gather';       note='เก็บของธรรมชาติ 1 ชิ้น'},
                    @{key='Butchery';     note='แล่ซาก 1 ชิ้นส่วน'},
                    @{key='Craft';        note='คราฟต์สำเร็จ 1 ครั้ง'},
                    @{key='Build';        note='สร้างเสร็จ 1 หลัง'},
                    @{key='SkillPointsPerLevel'; note='ขึ้นเลเวลแล้วได้แต้มสกิลกี่แต้ม'}
                ) 'เรท exp'
                if ($changed) { Write-Config $cfg }
            }
            '7' {
                $changed = Edit-Numbers $cfg.Skills @(
                    @{key='FullAt';        note='รวมเลเวลสกิลในหมวดถึงเท่านี้ = ได้โบนัสเต็ม'},
                    @{key='GatherSpeed';   note='เก็บของเร็วขึ้นสูงสุด (0.4 = 40%)'},
                    @{key='GatherBonus';   note='โอกาสได้ของเพิ่ม 1 ชิ้น สูงสุด'},
                    @{key='ButcherySpeed'; note='แล่ซากเร็วขึ้นสูงสุด'},
                    @{key='ButcheryBonus'; note='โอกาสได้ชิ้นส่วนเพิ่ม สูงสุด'},
                    @{key='MeleeDamage';   note='ดาเมจที่ตีออกเพิ่มสูงสุด'},
                    @{key='DefenseReduce'; note='ดาเมจที่รับลดลงสูงสุด'},
                    @{key='CraftSpeed';    note='คราฟต์เร็วขึ้นสูงสุด'},
                    @{key='StaminaSave';   note='ประหยัดสตามินาสูงสุด'},
                    @{key='RequiredPlayerLevelPerSkillLevel'; note='สกิลเลเวล N ต้องมีเลเวลผู้เล่น N x ค่านี้'}
                ) 'ผลของสกิล'
                if ($changed) { Write-Config $cfg }
            }
            '5' {
                Start-Process notepad.exe $configFile
                Say 'แก้แล้วกด Ctrl+S — เซิร์ฟจะอ่านใหม่เองภายใน 5 วินาที' 'DarkGray'
                Say 'ถ้าพิมพ์ JSON ผิด เซิร์ฟจะเตือนใน log แล้วใช้ค่าเดิมต่อ (ไม่ล่ม)' 'DarkGray'
            }
            '6' {
                $ans = Read-Host '  พิมพ์ DEFAULT เพื่อยืนยันการคืนค่าเริ่มต้น'
                if ($ans -eq 'DEFAULT') {
                    Copy-Item $configFile "$configFile.bak" -Force
                    Remove-Item $configFile -Force
                    Say 'ลบไฟล์แล้ว (สำรองเป็น config.json.bak) — เปิดเซิร์ฟใหม่จะสร้างค่าเริ่มต้นให้' 'Green'
                    if (Get-ServerProc) { Say 'ต้องปิด-เปิดเซิร์ฟใหม่ถึงจะได้ค่าเริ่มต้น (ข้อ 88 แล้ว 2)' 'Yellow' }
                } else { Say 'ยกเลิกแล้ว' }
            }
            '0' { return }
            default { Say 'ไม่มีข้อนี้' 'Red' }
        }
        Say ''
        Read-Host '  กด Enter เพื่อไปต่อ' | Out-Null
    }
}

# ───────────────────────── เครื่องมืออื่น ─────────────────────────

function Backup-Saves {
    if (-not (Test-Path $saves)) { Say 'ยังไม่มีโฟลเดอร์เซฟ' 'Red'; return $null }
    if (-not (Test-Path $backups)) { New-Item -ItemType Directory -Path $backups | Out-Null }
    $stamp = Get-Date -Format 'yyyyMMdd-HHmm'
    $zip = Join-Path $backups "saves-$stamp.zip"
    Compress-Archive -Path (Join-Path $saves '*') -DestinationPath $zip -Force
    $kb = [math]::Round((Get-Item $zip).Length / 1KB)
    Say "สำรองเซฟแล้ว: $zip ($kb KB)" 'Green'
    return $zip
}

function Reset-World {
    Say 'รีเซ็ตโลก = ลบไฟล์เซฟทั้งหมด (โลก + ตัวละครทุกคน) แล้วเริ่มเกาะใหม่หมด' 'Yellow'
    Say 'ระบบจะสำรองเป็น .zip ให้ก่อนเสมอ' 'DarkGray'
    $ans = Read-Host '  พิมพ์ RESET เพื่อยืนยัน (อย่างอื่น = ยกเลิก)'
    if ($ans -ne 'RESET') { Say 'ยกเลิกแล้ว' ; return }
    if (Get-ServerProc) { Say 'ปิดเซิร์ฟก่อน...' 'Yellow'; Stop-Server }
    if (-not (Backup-Saves)) { return }
    Remove-Item (Join-Path $saves 'world.json') -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $saves 'players\*') -Force -Recurse -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $saves 'accounts\*') -Force -Recurse -ErrorAction SilentlyContinue
    Say 'ลบเซฟแล้ว — เปิดเซิร์ฟใหม่จะได้เกาะสด ๆ' 'Green'
}

function Manage-Whitelist {
    while ($true) {
        Say ''
        Say "  รายชื่อที่อนุญาต ($whitelist)" 'Cyan'
        if (Test-Path $whitelist) {
            Get-Content $whitelist -Encoding UTF8 | ForEach-Object { Say "    $_" }
        } else { Say '    (ยังไม่มีไฟล์)' 'DarkGray' }
        Say ''
        Say '   1 เพิ่มชื่อ    2 ลบชื่อ    0 กลับ'
        $p = (Read-Host '  เลือก').Trim()
        switch ($p) {
            '1' {
                $n = (Read-Host '  ชื่อตัวละคร หรือ entity id').Trim()
                if ($n) {
                    Add-Content -Path $whitelist -Value $n -Encoding UTF8
                    Say "เพิ่ม $n แล้ว — ไฟล์นี้ hot-reload ไม่ต้องรีสตาร์ทเซิร์ฟ" 'Green'
                }
            }
            '2' {
                $n = (Read-Host '  ชื่อที่จะลบ').Trim()
                if ($n -and (Test-Path $whitelist)) {
                    $keep = Get-Content $whitelist -Encoding UTF8 | Where-Object { $_.Trim() -ne $n }
                    Set-Content -Path $whitelist -Value $keep -Encoding UTF8
                    Say "ลบ $n แล้ว" 'Green'
                }
            }
            '0' { return }
            default { Say 'ไม่มีข้อนี้' 'Red' }
        }
    }
}

function Show-GameLog {
    $logs = @(Get-ChildItem (Join-Path $game '*.log') -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending)
    if ($logs.Count -eq 0) { Say 'ยังไม่มี log ของตัวเกม (เปิดเกมสักครั้งก่อน)' 'Yellow'; return }
    $log = $logs[0]
    Say "log ล่าสุด: $($log.Name) (แก้ไขเมื่อ $($log.LastWriteTime))" 'Cyan'
    Say '--- 40 บรรทัดท้าย ---' 'DarkGray'
    Get-Content $log.FullName -Tail 40 | ForEach-Object {
        if ($_ -match 'Exception|Error|error') { Say "  $_" 'Red' } else { Say "  $_" 'DarkGray' }
    }
}

function Start-SoakTest {
    Say 'โซกเทส 30 นาที: บอทฟาร์ม 3 ตัวรัวเก็บของพร้อมกัน (เกณฑ์ข้อ 2 ของ beta)' 'Cyan'
    Say 'เปิดเป็น 3 หน้าต่างแยก — ระหว่างนี้ดูหน้าต่างเซิร์ฟว่า tps ตกหรือมี [error] ไหม' 'DarkGray'
    foreach ($n in 1..3) {
        Start-Process -FilePath 'dotnet' `
            -ArgumentList @('run','--no-build','--','--bot','127.0.0.1',"$GamePort",'30',"farmbot-$n") `
            -WorkingDirectory $tester -WindowStyle Normal | Out-Null
        Start-Sleep -Milliseconds 800
    }
    Say 'ปล่อยไว้ 30 นาที · เกณฑ์ผ่าน: exception 0 · tps ≥ 100 · RAM ไม่โตเกิน 20%' 'Green'
}

function Open-Folder {
    Say '   1 โฟลเดอร์เซฟ   2 โฟลเดอร์ doc   3 โฟลเดอร์เกม   4 โฟลเดอร์โปรเจกต์   5 ที่เก็บไฟล์สำรอง'
    $p = (Read-Host '  เลือก').Trim()
    $path = switch ($p) {
        '1' { $saves } '2' { Join-Path $root 'docs' } '3' { $game } '4' { $root } '5' { $backups }
        default { $null }
    }
    if ($path -and (Test-Path $path)) { Start-Process explorer.exe $path } else { Say 'ไม่มีโฟลเดอร์นี้' 'Red' }
}

function Show-Checklist {
    Say ''
    Say '  เช็คลิสต์เล่นจริง 30 นาที (เกณฑ์ข้อ 3 ของ beta 1.0)' 'Cyan'
    Say '  ---------------------------------------------------'
    Say '   [ ] คลิกที่สัตว์แล้ว "ปุ่มโจมตีสีแดง" เด้งขึ้นมา'
    Say '   [ ] ตีสัตว์แล้วมันสวนกลับ/วิ่งหนีภายใน ~1 วินาที (ไม่ยืนนิ่ง)'
    Say '   [ ] สัตว์ไม่วาร์ป ไม่ค้างท่า ตายแล้วนอนนิ่ง (ไม่ล้มแล้วลุกวน)'
    Say '   [ ] ฆ่าแล้วซาก "เรืองแสง" ขอบวิบวับ'
    Say '   [ ] แตะซาก (ต้องออกจากโหมดต่อสู้ก่อน) มีเมนู เนื้อ/หนัง/กระดูก'
    Say '   [ ] แล่แล้วของเข้ากระเป๋าจริง และแล่จนหมดตัวแล้วซากหายไป'
    Say '   [ ] ตายแล้วกดฟื้น จอเด้งกลับจุดเกิดจริง'
    Say '   [ ] ออกจากโหมดต่อสู้ได้ (ปุ่มถอย/Esc)'
    Say '   [ ] กระเป๋าเต็มแล้วกด "ทิ้ง" ได้'
    Say '   [ ] กินของแล้วสตามินาขึ้น'
    Say '   [ ] เก็บของธรรมชาติ + คราฟต์ + สร้างกองไฟได้'
    Say '   [ ] ตลอด 30 นาที หน้าต่างเซิร์ฟไม่มี [error] / exception'
    Say ''
    Say '  ทางลัด: ไม่ต้องเดินหาสัตว์ — ใช้ข้อ 5 (กล่องเครื่องมือ) เสกมาข้างตัวได้เลย' 'DarkGray'
}

function Show-CookChecklist {
    Say ''
    Say '  เช็คลิสต์เล่นจริง — ระบบทำอาหาร (~15 นาที)' 'Cyan'
    Say '  รายละเอียดเต็ม: docs\TESTPLAN.md หัวข้อ B' 'DarkGray'
    Say '  ---------------------------------------------------'
    Say '  เตรียม: ข้อ 5 (กล่องเครื่องมือ) -> เสกของ -> ชุดทำอาหาร' 'DarkGray'
    Say ''
    Say '  ไฟบังคับจริงไหม'
    Say '   [ ] วางกองไฟแล้วขึ้นเป็นกองไฟจริง (ไม่ใช่โครงร่างโปร่ง)'
    Say '   [ ] ยืนไกลกองไฟ -> สูตรย่างไม้เสียบ กดไม่ได้ (เทา)'
    Say '   [ ] ยืนติดกองไฟ -> สูตรเดิม กดได้'
    Say ''
    Say '  เครื่องมือบังคับจริงไหม'
    Say '   [ ] เอากิ่งไม้ออกจากกระเป๋า -> ย่างไม่ได้ ขึ้นหน้าต่างเครื่องมือ'
    Say '   [ ] เอาหม้อออก -> สูตรต้ม ขึ้นหน้าต่างเครื่องมือเหมือนกัน'
    Say ''
    Say '  ย่างแล้วได้อะไร'
    Say '   [ ] หลอดเวลาเดินจริง แล้วของเข้ากระเป๋า'
    Say '   [ ] ชื่อ+ไอคอนเปลี่ยน · คุณสมบัติ ดิบ (날 것) หายไป'
    Say '   [ ] เนื้อดิบลดลง 1 ก้อน · สตามินาลดตอนย่าง'
    Say ''
    Say '  กินแล้วต่างกันจริงไหม  <-- ข้อสำคัญที่สุด' 'White'
    Say '   [ ] ทำสตามินาหมด แล้วกินเนื้อดิบ -> ขึ้นราว +19'
    Say '   [ ] กินซ้ำทันที -> กินไม่ได้ (เพิ่งกินไป รออีกสักครู่)'
    Say '   [ ] รอ 5 วิ ทำหมดใหม่ กินเนื้อย่าง -> ขึ้นราว +32 (มากกว่าดิบชัดเจน)'
    Say '   [ ] กินของเหลว (น้ำ/ซุป) ใช้ท่าดื่ม ไม่ใช่ท่าเคี้ยว'
    Say ''
    Say '  เตาไล่ระดับจริงไหม'
    Say '   [ ] ที่กองไฟธรรมดา สูตรน้ำซุป กดไม่ได้'
    Say '   [ ] วางกองไฟใหญ่ แล้วยืนที่กองไฟใหญ่ -> น้ำซุป กดได้'
    Say '   [ ] เนื้อ 3 + น้ำ 1 -> ได้น้ำซุปเนื้อ 2 ถ้วย'
    Say '   [ ] ใส่ผัก/ผลไม้แทนเนื้อ -> ได้ซุปคนละอย่าง (ชื่อ/ไอคอนต่าง)'
    Say ''
    Say '  ของสุกอยู่ข้ามเซสชันไหม'
    Say '   [ ] เก็บเนื้อย่างไว้ -> ออกเกม -> เข้าใหม่ -> ยังเป็นเนื้อย่าง'
    Say ''
    Say '  ไม่พังของเดิม'
    Say '   [ ] คราฟต์มีดหิน/ขวาน ยังทำได้เหมือนเดิม'
    Say '   [ ] หน้าต่างเซิร์ฟไม่มี [error] / exception'
    Say ''
}

function Show-Status {
    Say ''
    $sp = Get-ServerProc
    if ($sp) {
        $mode = if ((Get-ServerArgs) -match '--enable-cheat') { 'โหมดเทส' } else { 'โหมดเปิดจริง' }
        Say "  เซิร์ฟ    : เปิดอยู่ ($mode, PID $($sp.Id))" 'Green'
    } else { Say '  เซิร์ฟ    : ปิดอยู่' 'DarkGray' }
    if (Test-Port $GamePort)    { Say "  พอร์ต $GamePort : ฟังอยู่" 'Green' } else { Say "  พอร์ต $GamePort : ไม่มีใครฟัง" 'DarkGray' }
    if (Test-Port $GatewayPort) { Say "  พอร์ต $GatewayPort : ฟังอยู่" 'Green' } else { Say "  พอร์ต $GatewayPort : ไม่มีใครฟัง" 'DarkGray' }
    $g = @(Get-GameProc)
    if ($g.Count -eq 0) { Say '  ตัวเกม    : ไม่ได้เปิด' 'DarkGray' }
    elseif ($g.Count -eq 1) { Say '  ตัวเกม    : เปิดอยู่ 1 ตัว' 'Green' }
    else { Say "  ตัวเกม    : เปิดอยู่ $($g.Count) ตัว — ต้องเหลือตัวเดียว!" 'Red' }
    Say ''
}

# ───────────────────────── เมนูหลัก ─────────────────────────

function Show-Menu {
    Clear-Host
    Say '╔══════════════════════════════════════════════════╗' 'Cyan'
    Say '║          Durango Claude — เมนูเทส                ║' 'Cyan'
    Say '╚══════════════════════════════════════════════════╝' 'Cyan'
    Show-Status
    Say '  --- เล่นเทสเอง ---------------------------------'
    Say '   1  เปิดเซิร์ฟ + เปิดเกม + ต่อให้เลย  (ใช้อันนี้)' 'White'
    Say '   2  เปิดเซิร์ฟอย่างเดียว (โหมดเทส cheat เปิด)'
    Say '   3  เปิดเกมอย่างเดียว (เซิร์ฟต้องเปิดอยู่แล้ว)'
    Say '   4  เช็คลิสต์ 30 นาที ต้องดูอะไรบ้าง'
    Say '   22 เช็คลิสต์ระบบทำอาหาร (เล่นจริง ~15 นาที)' 'White'
    Say '   5  กล่องเครื่องมือตอนเล่น (เสกสัตว์/ฆ่า/เติมเลือด/เสกของ)' 'White'
    Say ''
    Say '  --- เทสอัตโนมัติ -------------------------------'
    Say '   6  เทสกันโกง 45 ข้อ  (--gp-check)'
    Say '   20 เทสระบบทำอาหาร 11 ข้อ  (--cook-check)' 'White'
    Say '   21 ตรวจข้อมูลสูตร/อาหาร (ไม่ต้องเปิดเซิร์ฟ)'
    Say '   7  เทส 3 คนพร้อมกัน  (--multi-check)'
    Say '   8  บอทฟาร์ม 5 นาที'
    Say '   9  โซกเทส 30 นาที (บอท 3 ตัว) — เกณฑ์ข้อ 2'
    Say ''
    Say '  --- เครื่องมือ ---------------------------------'
    Say '   17 ตั้งค่าเซิร์ฟ: เรทเกิดสัตว์ · สมดุล · exp (แก้สด ๆ ได้)' 'White'
    Say '   18 build ตัวเกมจากซอร์ส client\ แล้ววางลงเกม' 'White'
    Say '   19 ย้อน DLL ตัวเกมกลับอันก่อนหน้า'
    Say '   10 บอทคอนโซล (พิมพ์คำสั่งเอง)'
    Say '   11 ดูใครออนไลน์'
    Say '   12 สำรองเซฟเป็น .zip'
    Say '   13 รีเซ็ตโลกใหม่ (สำรองให้ก่อน)'
    Say '   14 จัดการรายชื่อที่อนุญาต (whitelist)'
    Say '   15 ดู log ของตัวเกม (ตอนเกมเด้ง)'
    Say '   16 เปิดโฟลเดอร์'
    Say ''
    Say '   88 ปิดเซิร์ฟ      99 เปิดเซิร์ฟโหมดเปิดจริง      0 ออก'
    Say ''
}

while ($true) {
    Show-Menu
    $pick = (Read-Host '  เลือกข้อ').Trim()
    Say ''
    switch ($pick) {
        '1'  { if (Start-TestServer) { Start-Game; Show-Checklist } }
        '2'  { Start-TestServer | Out-Null }
        '3'  { Start-Game }
        '4'  { Show-Checklist }
        '22' { Show-CookChecklist; Read-Host '  กด Enter เพื่อกลับเมนู' | Out-Null }
        '5'  { if (Start-TestServer) { Enter-GmToolbox } }
        '6'  { if (Start-TestServer) { Invoke-Tester @('--gp-check') 'เทสกันโกง 45 ข้อ' } }
        '20' { if (Start-TestServer) { Invoke-Tester @('--cook-check') 'เทสระบบทำอาหาร 11 ข้อ' } }
        '21' {
            Say 'ตรวจข้อมูลสูตรคราฟต์/ทำอาหาร (ไม่ต้องเปิดเซิร์ฟ)' 'Cyan'
            Push-Location $server
            & dotnet run --no-build -- --recipe-check
            Pop-Location
            Say ''
            Read-Host '  กด Enter เพื่อกลับเมนู' | Out-Null
        }
        '7'  { if (Start-TestServer) { Invoke-Tester @('--multi-check') 'เทส 3 คนพร้อมกัน' } }
        '8'  { if (Start-TestServer) { Invoke-Tester @('--bot','127.0.0.1',"$GamePort",'5','farmbot-1') 'บอทฟาร์ม 5 นาที' } }
        '9'  { if (Start-TestServer) { Start-SoakTest } }
        '10' {
            if (Start-TestServer) {
                Say 'บอทคอนโซล — พิมพ์ help ดูคำสั่งทั้งหมด, quit เพื่อออก' 'Cyan'
                Push-Location $tester
                & dotnet run --no-build -- --console 127.0.0.1 $GamePort $GmBot
                Pop-Location
            }
        }
        '11' { if (Start-TestServer) { Show-Who } }
        '12' { Backup-Saves | Out-Null }
        '13' { Reset-World }
        '14' { Manage-Whitelist }
        '15' { Show-GameLog }
        '16' { Open-Folder }
        '17' { Enter-ConfigEditor }
        '18' {
            Say 'แก้ซอร์สใน client\ แล้ว build ใหม่ — ไม่ต้องใช้ IL patch อีกแล้ว' 'DarkGray'
            & (Join-Path $PSScriptRoot 'build-client.ps1')
        }
        '19' { & (Join-Path $PSScriptRoot 'build-client.ps1') -Restore }
        '88' { Stop-Server }
        '99' {
            if (Get-ServerProc) { Say 'ปิดเซิร์ฟตัวเทสก่อน' 'Yellow'; Stop-Server }
            Start-Server @('--whitelist','data/whitelist.txt') 'โหมดเปิดจริง (whitelist, cheat ปิด)' | Out-Null
            Say 'อ่าน docs/BETA-OPS.md ก่อนเปิดให้คนนอกเข้า' 'Yellow'
        }
        '0'  { Say 'จบแล้ว (เซิร์ฟยังเปิดอยู่ถ้าไม่ได้กดข้อ 88)' 'DarkGray'; exit 0 }
        default { Say 'ไม่มีข้อนี้' 'Red' }
    }
    Say ''
    Read-Host '  กด Enter เพื่อกลับเมนู' | Out-Null
}
