param(
  [string]$Port = "8193",
  [string]$Token = "",
  [string]$Path = "",
  [string]$Command = "",
  [string]$Kind = "",
  [string]$EntityId = "",
  [string]$X = "",
  [string]$Y = "",
  [switch]$Capture
)
$ErrorActionPreference = 'Stop'
$client = New-Object System.Net.Sockets.TcpClient('127.0.0.1', [int]$Port)
$stream = $client.GetStream()
$writer = New-Object System.IO.StreamWriter($stream, [System.Text.Encoding]::UTF8)
$reader = New-Object System.IO.StreamReader($stream, [System.Text.Encoding]::UTF8)
$writer.AutoFlush = $true
$id = [Guid]::NewGuid().ToString('N')
if ($Command) {
  $request = @{ request_id=$id; op='command'; name=$Command }
  if ($Kind) { $request.kind = $Kind }
  if ($EntityId) { $request.entity_id = $EntityId }
  if ($X -ne "") { $request.x = [double]$X }
  if ($Y -ne "") { $request.y = [double]$Y }
} elseif ($Capture) {
  $request = @{ request_id=$id; op='capture'; filename='memorybot-capture.png' }
} else {
  if (-not $Path) { $Path = 'game' }
  $request = @{ request_id=$id; op='read'; path=$Path }
}
if ($Token) { $request.token = $Token }
$writer.WriteLine(($request | ConvertTo-Json -Compress))
$result = $reader.ReadLine()
Write-Output $result
$client.Close()
