# FusionOS Database & Migration Architecture

## Overview

FusionOS uses **PostgreSQL 16** as its primary relational store, managed via **Entity Framework Core 8**.

## DbContext Per Module

Each module owns its own DbContext and isolated schema/tables:
- `CoreDbContext` (`Core` module)
- `FinanceDbContext` (`Finance` module)
- `InventoryDbContext` (`Inventory` module)
- `ManufacturingDbContext` (`Manufacturing` module)
- `WarehouseDbContext` (`Warehouse` module)
- `ProcurementDbContext` (`Procurement` module)
- `SalesDbContext` (`Sales` module)
- `CrmDbContext` (`Crm` module)
- `HrmsDbContext` (`Hrms` module)
- `QualityDbContext` (`Quality` module)
- `MaintenanceDbContext` (`Maintenance` module)
- `BiDbContext` (`BusinessIntelligence` module)
- `AiDbContext` (`Ai` module)
- `IntegrationHubDbContext` (`IntegrationHub` module)
- `MarketplaceDbContext` (`Marketplace` module)

## Concurrency Control

PostgreSQL `xmin` concurrency tokens (`UseXminAsConcurrencyToken()`) are enabled across all entities to prevent lost updates in multi-tenant environments.

## Automated Migration Management

Use the PowerShell or Bash scripts in `scripts/`:

```powershell
# Generate EF Core migrations for all module DbContexts
./scripts/generate-migrations.ps1
```

```bash
# Take a raw Postgres dump backup
./scripts/backup-postgres.sh
```
