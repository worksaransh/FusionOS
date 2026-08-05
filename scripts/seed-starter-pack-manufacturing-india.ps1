# ------------------------------------------------------------------------------
# FusionOS - "Manufacturing (India)" Starter Pack Seeder
# ------------------------------------------------------------------------------
# Seeds a Schedule III-aligned Indian manufacturer Chart of Accounts, a GST tax
# jurisdiction, and GST tax rates (CGST/SGST/IGST @ 5/12/18/28%) into an
# EXISTING company via the real Finance REST API.
#
# Unlike scripts/seed-enterprise-tenant.ps1 (which logs in itself), this script
# takes CompanyId and a Bearer token directly - it is meant to be run against a
# company/user that already exists (e.g. right after self-serve trial signup),
# not to create one.
#
# Template data lives alongside this script in:
#   scripts/starter-packs/manufacturing-india/chart-of-accounts.json
#   scripts/starter-packs/manufacturing-india/tax-rates.json
#
# Usage:
#   ./scripts/seed-starter-pack-manufacturing-india.ps1 `
#       -CompanyId "11111111-2222-3333-4444-555555555555" `
#       -Token "eyJhbGciOi..." `
#       -BaseUrl "http://localhost:5000/api/v1"
#
# See docs/starter-packs/MANUFACTURING_INDIA.md for full details.
#
# IMPORTANT: This script only calls the REST API - it never touches the
# database directly, and running it is safe to re-run (duplicate Code values
# are rejected by the API and logged as [INFO], not treated as fatal).
# ------------------------------------------------------------------------------

param (
    [Parameter(Mandatory = $true)]
    [string]$CompanyId,

    [Parameter(Mandatory = $true)]
    [string]$Token,

    [string]$BaseUrl = "http://localhost:5000/api/v1"
)

$ErrorActionPreference = "Continue"

$headers = @{ Authorization = "Bearer $Token" }

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$packDir = Join-Path $scriptDir "starter-packs/manufacturing-india"

Write-Host "Starting FusionOS 'Manufacturing (India)' Starter Pack Seeding..." -ForegroundColor Cyan
Write-Host "CompanyId: $CompanyId" -ForegroundColor Cyan
Write-Host "BaseUrl:   $BaseUrl" -ForegroundColor Cyan

# ------------------------------------------------------------------------------
# 1. Seed Chart of Accounts
# ------------------------------------------------------------------------------
$coaPath = Join-Path $packDir "chart-of-accounts.json"
$coa = Get-Content -Raw -Path $coaPath | ConvertFrom-Json

Write-Host ""
Write-Host "Seeding Chart of Accounts ($($coa.accounts.Count) accounts)..." -ForegroundColor White
foreach ($a in $coa.accounts) {
    $body = @{
        companyId   = $CompanyId
        code        = $a.Code
        name        = $a.Name
        accountType = $a.AccountType
    } | ConvertTo-Json

    try {
        $res = Invoke-RestMethod -Uri "$BaseUrl/finance/accounts" -Method Post -Headers $headers -ContentType "application/json" -Body $body
        Write-Host "  [OK] Account created: $($a.Code) - $($a.Name) ($($a.AccountType))" -ForegroundColor Green
    } catch {
        Write-Host "  [INFO] Account $($a.Code): $_" -ForegroundColor Gray
    }
    Start-Sleep -Milliseconds 100
}

# ------------------------------------------------------------------------------
# 2. Seed GST Tax Jurisdiction
# ------------------------------------------------------------------------------
$taxPath = Join-Path $packDir "tax-rates.json"
$taxPack = Get-Content -Raw -Path $taxPath | ConvertFrom-Json

Write-Host ""
Write-Host "Seeding Tax Jurisdiction ($($taxPack.jurisdiction.Code))..." -ForegroundColor White

$jurisdictionId = $null
$jurisBody = @{
    companyId = $CompanyId
    code      = $taxPack.jurisdiction.Code
    name      = $taxPack.jurisdiction.Name
} | ConvertTo-Json

try {
    $jurisRes = Invoke-RestMethod -Uri "$BaseUrl/finance/tax-jurisdictions" -Method Post -Headers $headers -ContentType "application/json" -Body $jurisBody
    $jurisdictionId = $jurisRes.id
    Write-Host "  [OK] Tax jurisdiction created: $($taxPack.jurisdiction.Code) (id: $jurisdictionId)" -ForegroundColor Green
} catch {
    Write-Host "  [INFO] Tax jurisdiction $($taxPack.jurisdiction.Code): $_" -ForegroundColor Gray
}

# Fallback: if creation failed (e.g. already exists from a prior run), look it up.
if (-not $jurisdictionId) {
    try {
        $jurisListRes = Invoke-RestMethod -Uri "$BaseUrl/finance/tax-jurisdictions?companyId=$CompanyId&pageSize=100" -Headers $headers
        $match = $jurisListRes.data | Where-Object { $_.code -eq $taxPack.jurisdiction.Code }
        if ($match) {
            $jurisdictionId = $match.id
            Write-Host "  [INFO] Resolved existing tax jurisdiction id: $jurisdictionId" -ForegroundColor Gray
        }
    } catch {
        Write-Host "  [INFO] Tax jurisdiction lookup: $_" -ForegroundColor Gray
    }
}

# ------------------------------------------------------------------------------
# 3. Seed GST Tax Rates (CGST / SGST / IGST @ 5% / 12% / 18% / 28%)
# ------------------------------------------------------------------------------
Write-Host ""
if (-not $jurisdictionId) {
    Write-Host "Skipping tax rate seeding - no tax jurisdiction id available." -ForegroundColor Red
} else {
    Write-Host "Seeding GST Tax Rates ($($taxPack.taxRates.Count) rates)..." -ForegroundColor White
    foreach ($t in $taxPack.taxRates) {
        $body = @{
            companyId         = $CompanyId
            taxJurisdictionId = $jurisdictionId
            code              = $t.Code
            name              = $t.Name
            percentage        = $t.Percentage
            gstComponent      = $t.GstComponent
        } | ConvertTo-Json

        try {
            $res = Invoke-RestMethod -Uri "$BaseUrl/finance/tax-rates" -Method Post -Headers $headers -ContentType "application/json" -Body $body
            Write-Host "  [OK] Tax rate created: $($t.Code) - $($t.Name) ($($t.Percentage)%)" -ForegroundColor Green
        } catch {
            Write-Host "  [INFO] Tax rate $($t.Code): $_" -ForegroundColor Gray
        }
        Start-Sleep -Milliseconds 100
    }
}

Write-Host ""
Write-Host "FusionOS 'Manufacturing (India)' Starter Pack Seeding Completed!" -ForegroundColor Green
