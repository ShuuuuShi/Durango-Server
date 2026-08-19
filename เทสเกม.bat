@echo off
REM ==========================================================
REM  Durango Claude - test menu (double-click me)
REM  Everything in Thai lives in tools\menu.ps1, not here:
REM  cmd.exe reads .bat as codepage 874/ANSI, so Thai text
REM  inside a UTF-8 .bat comes out as garbage.
REM ==========================================================
chcp 874 >nul 2>&1
title Durango Claude - test menu
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0tools\menu.ps1"
if errorlevel 1 (
  echo.
  echo [!] menu.ps1 failed to run - see the message above.
  pause
)
