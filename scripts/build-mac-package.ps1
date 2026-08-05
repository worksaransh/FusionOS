<#
Builds the macOS download packages for FusionOS. Since this repo has no access to Mac-only
packaging tools (pkgbuild/hdiutil/codesign/notarization), this produces a self-contained zip
per architecture instead of a signed .pkg/.dmg — see installer/mac/README.md for what that
means for the end user (Gatekeeper's unidentified-developer warning is expected).

Publishes for both osx-arm64 (Apple Silicon) and osx-x64 (Intel), builds the frontend once,
and copies both zips into frontend/public/downloads/ so the site's download buttons serve
the current build.

Run from the repo root: powershell -File scripts/build-mac-package.ps1
#>

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$downloadsDir = Join-Path $repoRoot "frontend\public\downloads"
$macSrc = Join-Path $repoRoot "installer\mac"

function Publish-Mac {
    param([string]$Rid, [string]$ZipName)

    $publishDir = Join-Path $repoRoot "build\publish\$Rid"
    Write-Host "== Publishing API host for $Rid ==" -ForegroundColor Cyan
    if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
    dotnet publish "$repoRoot\backend\src\Host\FusionOS.Api.Host\FusionOS.Api.Host.csproj" `
        -c Release -r $Rid --self-contained true `
        -o $publishDir
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for $Rid" }

    $devAppsettings = Join-Path $publishDir "appsettings.Development.json"
    if (Test-Path $devAppsettings) { Remove-Item $devAppsettings -Force }

    $wwwroot = Join-Path $publishDir "wwwroot"
    New-Item -ItemType Directory -Force -Path $wwwroot | Out-Null
    Copy-Item "$repoRoot\frontend\dist\*" $wwwroot -Recurse -Force

    Copy-Item "$macSrc\launch.sh" $publishDir -Force
    Copy-Item "$macSrc\stop.sh" $publishDir -Force
    Copy-Item "$macSrc\setup.sh" $publishDir -Force
    Copy-Item "$macSrc\README.md" $publishDir -Force

    Write-Host "== Zipping $ZipName ==" -ForegroundColor Cyan
    $zipPath = Join-Path $downloadsDir $ZipName
    if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
    Compress-Archive -Path "$publishDir\*" -DestinationPath $zipPath
}

Write-Host "== Building frontend (shared by both architectures) ==" -ForegroundColor Cyan
Push-Location "$repoRoot\frontend"
try {
    npm run build
    if ($LASTEXITCODE -ne 0) { throw "npm run build failed" }
} finally {
    Pop-Location
}

New-Item -ItemType Directory -Force -Path $downloadsDir | Out-Null

Publish-Mac -Rid "osx-arm64" -ZipName "FusionOS-Mac-AppleSilicon.zip"
Publish-Mac -Rid "osx-x64" -ZipName "FusionOS-Mac-Intel.zip"

Write-Host "Done. Packages in: $downloadsDir" -ForegroundColor Green
