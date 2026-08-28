$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'SaveBackup.ps1')

$root = Join-Path ([IO.Path]::GetTempPath()) ('durango-backup-check-' + [guid]::NewGuid().ToString('N'))
$save = Join-Path $root 'saves'
$backups = Join-Path $root 'backups'
$restore = Join-Path $root 'restore'
try {
    $files = @{
        'world.json' = '{"world":"single"}'
        'worlds/island-a.json' = '{"world":"island"}'
        'players/player-a.json' = '{"player":"a"}'
        'accounts/player-a.json' = '{"account":"a"}'
        'mods/examplemod/install.json' = '{"mod":"ok"}'
        'future/nested/data.json' = '{"future":true}'
    }
    foreach ($relative in $files.Keys) {
        $path = Join-Path $save $relative
        New-Item -ItemType Directory -Force -Path ([IO.Path]::GetDirectoryName($path)) | Out-Null
        [IO.File]::WriteAllText($path, $files[$relative])
    }
    $snapshot = New-SaveSnapshot $save $backups
    if (-not (Test-Path $snapshot.Archive) -or [string]::IsNullOrEmpty($snapshot.Sha256)) { throw 'สร้าง archive หรือ hash ไม่สำเร็จ' }
    Test-SaveSnapshot $snapshot.Archive $save | Out-Null
    Expand-SaveSnapshot $snapshot.Archive $restore | Out-Null
    foreach ($relative in $files.Keys) {
        $source = [IO.File]::ReadAllBytes((Join-Path $save $relative))
        $restored = [IO.File]::ReadAllBytes((Join-Path $restore $relative))
        if ($source.Length -ne $restored.Length -or [Convert]::ToBase64String($source) -ne [Convert]::ToBase64String($restored)) { throw "restore ไม่ตรง: $relative" }
    }
    [IO.File]::WriteAllText((Join-Path $save 'players/player-a.json'), '{"player":"changed-after-snapshot"}')
    if ([IO.File]::ReadAllText((Join-Path $restore 'players/player-a.json')) -ne '{"player":"a"}') { throw 'restore ปนการเปลี่ยนหลัง snapshot' }
    Write-Host "[PASS] backup/restore workflow: $($snapshot.FileCount) files, SHA256 $($snapshot.Sha256)" -ForegroundColor Green
    exit 0
} finally {
    Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue
}
