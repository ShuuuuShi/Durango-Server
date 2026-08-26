param(
  [string]$Port = "8193",
  [string]$Token = "",
  [string]$Path = "game",
  [switch]$Capture
)
$ErrorActionPreference = 'Stop'
$client = New-Object System.Net.Sockets.TcpClient('127.0.0.1', [int]$Port)
$stream = $client.GetStream()
$writer = New-Object System.IO.StreamWriter($stream, [System.Text.Encoding]::UTF8)
$reader = New-Object System.IO.StreamReader($stream, [System.Text.Encoding]::UTF8)
$writer.AutoFlush = $true
$id = [Guid]::NewGuid().ToString('N')
if ($Capture) {
  $request = @{ request_id=$id; op='capture'; filename='memorybot-capture.png' }
} else {
  $request = @{ request_id=$id; op='read'; path=$Path }
}
if ($Token) { $request.token = $Token }
$writer.WriteLine(($request | ConvertTo-Json -Compress))
$result = $reader.ReadLine()
Write-Output $result
$client.Close()
