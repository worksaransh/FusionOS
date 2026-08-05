<#
Builds the Windows desktop installer for FusionOS:
  1. Publishes the API host as a self-contained win-x64 app (serves the SPA itself — see
     Program.cs's servesSpa block — so no separate IIS/nginx is needed on the target machine).
  2. Builds the frontend and copies its output into the publish folder's wwwroot/.
  3. Compiles installer/FusionOS.iss with Inno Setup's ISCC.exe into a single .exe installer.
  4. Copies the compiled installer into frontend/public/downloads/ so the marketing site's
     "Download for Windows" button serves a real, current artifact.

Run from the repo root: powershell -File scripts/build-windows-installer.ps1
#>

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $repoRoot "build\publish\api"
$installerDir = Join-Path $repoRoot "installer"
$downloadsDir = Join-Path $repoRoot "frontend\public\downloads"

Write-Host "== 1/4 Publishing API host (self-contained win-x64) ==" -ForegroundColor Cyan
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
dotnet publish "$repoRoot\backend\src\Host\FusionOS.Api.Host\FusionOS.Api.Host.csproj" `
    -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=false `
    -o $publishDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

# Dev-only appsettings ships dev placeholder secrets — never in an installed package.
$devAppsettings = Join-Path $publishDir "appsettings.Development.json"
if (Test-Path $devAppsettings) { Remove-Item $devAppsettings -Force }

Write-Host "== 2/4 Building frontend ==" -ForegroundColor Cyan
Push-Location "$repoRoot\frontend"
try {
    npm run build
    if ($LASTEXITCODE -ne 0) { throw "npm run build failed" }
} finally {
    Pop-Location
}

$wwwroot = Join-Path $publishDir "wwwroot"
New-Item -ItemType Directory -Force -Path $wwwroot | Out-Null
Copy-Item "$repoRoot\frontend\dist\*" $wwwroot -Recurse -Force

Write-Host "== 3/4 Compiling installer with Inno Setup ==" -ForegroundColor Cyan
$iscc = "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
if (-not (Test-Path $iscc)) { $iscc = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" }
if (-not (Test-Path $iscc)) { throw "ISCC.exe not found - install Inno Setup 6 first (winget install JRSoftware.InnoSetup)" }

& $iscc "$installerDir\FusionOS.iss"
if ($LASTEXITCODE -ne 0) { throw "ISCC compile failed" }

Write-Host "== 4/4 Publishing installer artifact for web download ==" -ForegroundColor Cyan
New-Item -ItemType Directory -Force -Path $downloadsDir | Out-Null
Copy-Item "$installerDir\Output\FusionOS-Setup.exe" (Join-Path $downloadsDir "FusionOS-Setup.exe") -Force

Write-Host "Done. Installer at: $downloadsDir\FusionOS-Setup.exe" -ForegroundColor Green
