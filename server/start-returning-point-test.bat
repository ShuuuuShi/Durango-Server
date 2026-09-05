@echo off
cd /d "C:\Users\thana\Desktop\Durango Opencode\server"
"bin\Debug\net9.0\DurangoServer.exe" --game-port 8291 --gateway-port 8290 --saves saves-local-client --admin-token chunktest --enable-cheat --client-mod-allowlist "data\client-mod-allowlist.json" >> "realtest-returning-point3.log" 2>&1
