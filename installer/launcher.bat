@echo off
rem Starts the FusionOS desktop server minimized and opens it in the default browser.
set ASPNETCORE_ENVIRONMENT=Production
cd /d "%~dp0"
start /min "FusionOS" "%~dp0FusionOS.Api.Host.exe"
timeout /t 3 /nobreak >nul
start http://localhost:5000
