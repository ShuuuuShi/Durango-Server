Set-StrictMode -Version Latest

function Get-SaveFiles([string]$SaveRoot) {
    if (-not (Test-Path -LiteralPath $SaveRoot -PathType Container)) { throw "ไม่พบ save root: $SaveRoot" }
    @(Get-ChildItem -LiteralPath $SaveRoot -File -Recurse | Sort-Object FullName)
}

function New-SaveSnapshot([string]$SaveRoot, [string]$BackupRoot) {
    $save = [IO.Path]::GetFullPath($SaveRoot)
    $backup = [IO.Path]::GetFullPath($BackupRoot)
    if ($backup.StartsWith($save + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw 'backup root ห้ามอยู่ใต้ save root' }
    $files = Get-SaveFiles $save
    if ($files.Count -eq 0) { throw 'save root ว่าง' }
    New-Item -ItemType Directory -Force -Path $backup | Out-Null
    $archive = Join-Path $backup ("saves-" + (Get-Date -Format 'yyyyMMdd-HHmmssfff') + '.zip')
    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::Open($archive, [IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($file in $files) {
            $relative = $file.FullName.Substring($save.Length).TrimStart([char[]]'\/').Replace('\', '/')
            [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $file.FullName, $relative, [IO.Compression.CompressionLevel]::Optimal) | Out-Null
        }
    } finally { $zip.Dispose() }
    Test-SaveSnapshot $archive $save | Out-Null
    [pscustomobject]@{ Archive=$archive; Sha256=(Get-FileHash -Algorithm SHA256 -LiteralPath $archive).Hash; FileCount=$files.Count }
}

function Test-SaveSnapshot([string]$Archive, [string]$SaveRoot) {
    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $save = [IO.Path]::GetFullPath($SaveRoot)
    $files = Get-SaveFiles $save
    $zip = [IO.Compression.ZipFile]::OpenRead($Archive)
    try {
        $entries = @{}; foreach ($entry in $zip.Entries) { $entries[$entry.FullName] = $entry }
        foreach ($file in $files) {
            $relative = $file.FullName.Substring($save.Length).TrimStart([char[]]'\/').Replace('\', '/')
            if (-not $entries.ContainsKey($relative) -or $entries[$relative].Length -ne $file.Length) { throw ('archive ไม่ครบหรือขนาดไม่ตรง: {0}' -f $relative) }
        }
    } finally { $zip.Dispose() }
    $true
}

function Expand-SaveSnapshot([string]$Archive, [string]$Destination) {
    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $dest = [IO.Path]::GetFullPath($Destination)
    New-Item -ItemType Directory -Force -Path $dest | Out-Null
    $prefix = $dest.TrimEnd('\','/') + [IO.Path]::DirectorySeparatorChar
    $zip = [IO.Compression.ZipFile]::OpenRead($Archive)
    try {
        foreach ($entry in $zip.Entries) {
            $target = [IO.Path]::GetFullPath((Join-Path $dest $entry.FullName))
            if (-not $target.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) { throw "archive path ไม่ปลอดภัย: $($entry.FullName)" }
            if ([string]::IsNullOrEmpty($entry.Name)) { New-Item -ItemType Directory -Force -Path $target | Out-Null; continue }
            New-Item -ItemType Directory -Force -Path ([IO.Path]::GetDirectoryName($target)) | Out-Null
            [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $target, $true)
        }
    } finally { $zip.Dispose() }
    $dest
}
