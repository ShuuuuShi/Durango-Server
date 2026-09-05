# ตัวคุมเปิดเซิร์ฟใหม่เองเมื่อโปรเซสหาย (Windows)
# ใช้เมื่อไม่ได้เปิดผ่าน DevKit panel
#
# ตัวอย่าง:
#   powershell -File server\scripts\watch-server.ps1 -- `
#     --gateway-port 8290 --game-port 8291 --cluster-mode Online --loose-ip-match --enable-cheat

param(
    [string]$Exe = "",
    [int]$RestartDelaySec = 5
)

$ErrorActionPreference = "Stop"
if (-not $Exe) {
    $Exe = Join-Path $PSScriptRoot "..\bin\Release\net9.0\DurangoServer.exe"
}
$Exe = [IO.Path]::GetFullPath($Exe)
if (-not (Test-Path $Exe)) {
    Write-Error "ไม่พบ $Exe — ส่ง -Exe มาเอง หรือ build เซิร์ฟก่อน"
}

$serverDir = Split-Path $Exe -Parent
$pass = @()
foreach ($a in $args) { $pass += $a }

Write-Host "watch-server: $Exe"
Write-Host "args: $($pass -join ' ')"

while ($true) {
    $p = Start-Process -FilePath $Exe -ArgumentList $pass -WorkingDirectory $serverDir -PassThru -Wait
    $code = $p.ExitCode
    Write-Host "[watch] เซิร์ฟออกด้วย code=$code — เปิดใหม่ใน $RestartDelaySec วิ"
    Start-Sleep -Seconds $RestartDelaySec
}
