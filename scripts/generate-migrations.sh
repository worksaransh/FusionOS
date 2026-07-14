#!/usr/bin/env bash
# Generates the initial EF Core migration for every module that has real
# entities mapped (04_DATABASE_GUIDELINES.md). Must be run on a machine with
# the .NET 8 SDK — this sandbox has no `dotnet` binary, so these migrations
# have never actually been generated or applied; every module has been
# running against `EnsureCreated`-style implicit schema only in whatever
# manual testing has happened so far. Treat the output of this script as
# UNVERIFIED until someone runs it and applies it against a real Postgres
# instance (see docker-compose.yml for a local one).
#
# Usage:
#   ./scripts/generate-migrations.sh            # generate InitialCreate for every module below
#   ./scripts/generate-migrations.sh --apply     # also run `dotnet ef database update` after generating
set -euo pipefail
cd "$(dirname "$0")/.."

HOST_PROJECT="backend/src/Host/FusionOS.Api.Host/FusionOS.Api.Host.csproj"
APPLY=false
if [[ "${1:-}" == "--apply" ]]; then
  APPLY=true
fi

if ! dotnet tool list --global | grep -q dotnet-ef; then
  echo "Installing dotnet-ef (dotnet tool install --global dotnet-ef)..."
  dotnet tool install --global dotnet-ef
fi

# Module -> DbContext, matched 1:1 with the six modules that have real
# EF configurations today (04_DATABASE_GUIDELINES.md §1). The other nine
# modules (Ai, BusinessIntelligence, Crm, Hrms, IntegrationHub, Maintenance,
# Manufacturing, Marketplace, Quality) have zero DbSets — running migrations
# against them would produce empty, meaningless migrations, so they are
# deliberately excluded until each gets a real vertical slice.
declare -A MODULE_CONTEXTS=(
  [Core]="CoreDbContext"
  [Inventory]="InventoryDbContext"
  [Warehouse]="WarehouseDbContext"
  [Procurement]="ProcurementDbContext"
  [Sales]="SalesDbContext"
  [Finance]="FinanceDbContext"
)

for module in "${!MODULE_CONTEXTS[@]}"; do
  context="${MODULE_CONTEXTS[$module]}"
  project=$(find backend/src/Modules/"$module" -iname "*.Infrastructure.csproj" | head -n1)

  if [[ -z "$project" ]]; then
    echo "SKIP: no Infrastructure project found for module $module"
    continue
  fi

  echo "=== $module ($context) ==="
  dotnet ef migrations add InitialCreate \
    --project "$project" \
    --startup-project "$HOST_PROJECT" \
    --context "$context" \
    --output-dir Persistence/Migrations

  if $APPLY; then
    dotnet ef database update \
      --project "$project" \
      --startup-project "$HOST_PROJECT" \
      --context "$context"
  fi
done

echo "Done. Review every generated migration under Persistence/Migrations before applying to any real environment."
