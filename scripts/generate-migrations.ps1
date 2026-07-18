# Windows PowerShell Script to generate and optionally apply EF Core migrations for FusionOS modules.
# Usage:
#   .\scripts\generate-migrations.ps1
#   .\scripts\generate-migrations.ps1 -Apply

param (
    [switch]$Apply
)

# Move to the root directory of the repository (parent of the script directory)
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location "$ScriptDir\.."

$HostProject = "backend/src/Host/FusionOS.Api.Host/FusionOS.Api.Host.csproj"

# Check if dotnet-ef is installed
$tools = dotnet tool list --global
if ($tools -notmatch "dotnet-ef") {
    Write-Host "Installing dotnet-ef global tool..."
    dotnet tool install --global dotnet-ef
}

$ModuleContexts = @{
    "Core"          = "CoreDbContext"
    "Inventory"     = "InventoryDbContext"
    "Warehouse"     = "WarehouseDbContext"
    "Procurement"   = "ProcurementDbContext"
    "Sales"         = "SalesDbContext"
    "Finance"       = "FinanceDbContext"
    "Manufacturing" = "ManufacturingDbContext"
    "Crm"           = "CrmDbContext"
    "Quality"       = "QualityDbContext"
    "Maintenance"   = "MaintenanceDbContext"
    "Hrms"          = "HrmsDbContext"
    "BusinessIntelligence" = "BusinessIntelligenceDbContext"
    "Ai"            = "AiDbContext"
    "Marketplace"   = "MarketplaceDbContext"
    "IntegrationHub" = "IntegrationHubDbContext"
}

foreach ($item in $ModuleContexts.GetEnumerator()) {
    $module = $item.Key
    $context = $item.Value
    
    # Find the infrastructure project file
    $projectFile = Get-ChildItem -Path "backend/src/Modules/$module" -Filter "*.Infrastructure.csproj" -Recurse | Select-Object -First 1
    
    if (-not $projectFile) {
        Write-Host "SKIP: no Infrastructure project found for module $module" -ForegroundColor Yellow
        continue
    }

    $projectPath = $projectFile.FullName
    Write-Host "=== Module: $module ($context) ===" -ForegroundColor Cyan
    
    # Check if Migrations directory already exists to see if we should skip migrations add
    $migrationsDir = Join-Path (Split-Path $projectPath) "Persistence/Migrations"
    if (Test-Path $migrationsDir) {
        Write-Host "Migrations directory already exists at $migrationsDir. Skipping generation."
    } else {
        dotnet ef migrations add InitialCreate `
            --project "$projectPath" `
            --startup-project "$HostProject" `
            --context "$context" `
            --output-dir Persistence/Migrations
    }

    if ($Apply) {
        Write-Host "Applying database migrations to PostgreSQL..." -ForegroundColor Green
        dotnet ef database update `
            --project "$projectPath" `
            --startup-project "$HostProject" `
            --context "$context"
    }
}

Write-Host "Done. Review any generated migrations under the respective modules' Persistence/Migrations folder." -ForegroundColor Green
