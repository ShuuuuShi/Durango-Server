@echo off
REM Rebuild Assembly-CSharp.dll from the pristine original (.bak) and install it.
REM The .bak is the ONLY valid input - never patch an already-patched dll (patches would stack).
setlocal
set ROOT=%~dp0..
set MANAGED=%ROOT%\game\DurangoV2_Data\Managed

tasklist /fi "imagename eq DurangoV2.exe" | find /i "DurangoV2.exe" >nul
if not errorlevel 1 (
  echo [!] game is still running - close it first
  exit /b 1
)

if not exist "%MANAGED%\Assembly-CSharp.dll.bak" (
  echo [!] original not found: %MANAGED%\Assembly-CSharp.dll.bak
  exit /b 1
)

pushd "%~dp0DllPatcher"
dotnet run -- "%MANAGED%\Assembly-CSharp.dll.bak"
if errorlevel 1 (
  popd
  echo [!] patch failed
  exit /b 1
)
popd

copy /y "%MANAGED%\Assembly-CSharp.dll" "%MANAGED%\Assembly-CSharp.dll.prev" >nul
copy /y "%MANAGED%\Assembly-CSharp.dll.bak.patched.dll" "%MANAGED%\Assembly-CSharp.dll" >nul
echo [ok] installed (previous dll saved as Assembly-CSharp.dll.prev)
endlocal
