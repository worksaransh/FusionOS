# On-Prem Deployment Runbook — FusionOS

Status: written and, where marked, actually exercised — not aspirational. Companion to
`docs/blueprint/14_DEPLOYMENT_GUIDE.md` (local dev / full containerized stack) and
`docs/DISASTER_RECOVERY.md` (backup/DR scope). This document is specifically for a customer's own
IT team standing up FusionOS on their own hardware/VM, without the Kafka/observability stack the
root `docker-compose.yml` always brings up.

Do not duplicate or contradict `docs/blueprint/14_DEPLOYMENT_GUIDE.md` — that document is the
source of truth for the full containerized/dev stack and the environment-variable reference table.
This document only covers what's different for a trimmed on-prem install, plus the migration and
backup/restore steps a production release actually needs that the dev bootstrap papers over.

## 0. Windows desktop installer (single-machine, no Docker)

For a single Windows PC — the "download and run" path from the marketing site's **Download for Windows**
button — FusionOS ships a self-contained installer instead of the Docker Compose stack in §1. This is a
genuinely different deployment shape, not a shortcut for §1:

- The published API host serves the built SPA itself (`Program.cs`'s `servesSpa` block: `UseDefaultFiles`/
  `UseStaticFiles`/`MapFallbackToFile` against a `wwwroot/` folder copied in at publish time) — there is no
  separate nginx/IIS process, no separate frontend origin.
- Migrations run automatically on every start (`Deployment:AutoMigrate=true` in the installed
  `appsettings.Production.json`, read by the same `MigrateAsync` gate in `Program.cs` that §2 below documents
  for the Development-only default) — safe here because a single-machine desktop install has no separate
  release pipeline to run `dotnet ef database update` from.
- Kafka is not part of this package at all; the app's existing graceful-degradation behavior (connection
  retries fail silently, REST APIs and outbox polling keep working) applies exactly as it does everywhere else
  this session confirmed it.
- PostgreSQL is **not** bundled — the installer's wizard prompts for host/port/database/username/password
  (defaulting to a local `fusionos`/`fusionos` setup) and writes `appsettings.Production.json` with that
  connection string plus a freshly generated `Jwt:SigningKey` (48 random characters, unique per install).

**Building the installer:** `powershell -File scripts/build-windows-installer.ps1` from the repo root —
publishes the API host self-contained for `win-x64`, builds the frontend into that publish folder's
`wwwroot/`, compiles `installer/FusionOS.iss` with Inno Setup's `ISCC.exe`
(`winget install JRSoftware.InnoSetup` if not already installed), and copies the resulting
`FusionOS-Setup.exe` into `frontend/public/downloads/` so the site's download button serves the current build.

**Verified this session:** built the real installer end-to-end (47.7 MB), ran the published exe directly
(bypassing the GUI wizard, env vars set manually) against the live local Postgres with
`ASPNETCORE_ENVIRONMENT=Production` and `Deployment:AutoMigrate=true` — confirmed `/` and a client-side route
(`/dashboard`) both serve the real SPA shell (200, actual `index.html`), static assets serve correctly, and
the API (`/api/v1/core/auth/login`) still responds correctly through the same process. One real bug was found
and fixed in this pass: the initial `UseDefaultFiles()`/`UseStaticFiles()` (no explicit options) failed to
serve `/` — it fell through the whole pipeline and hit the global auth policy instead — root-caused to
`IWebHostEnvironment.WebRootPath` resolution being ambiguous for a self-contained single-exe publish;
fixed by binding both middlewares and the SPA fallback route to an explicit `PhysicalFileProvider` over the
computed `wwwroot` path instead of relying on that convention.

**Uninstall / stop:** Start Menu → FusionOS → Stop FusionOS (or Uninstall FusionOS, which also stops the
running process first via `taskkill`). The installed Postgres database is never touched by uninstall — only
the application files are removed.

## 1. The on-prem stack

`docker-compose.onprem.yml` (repo root) is a trimmed alternative to `docker-compose.yml`:

| Service | Always on? | Notes |
|---|---|---|
| `postgres` | Yes | Same image/config as the dev compose file, its own named volume. |
| `api` | Yes | Built from `backend/Dockerfile`. `ASPNETCORE_ENVIRONMENT=Production` (not `Development` — see §2). |
| `web` | Yes | Built from `frontend/Dockerfile`. Its nginx (`frontend/nginx.conf`) serves the built SPA **and** proxies `/api/` to the `api` container on the same origin — this already is the on-prem ingress point (port 80, or put your own TLS terminator in front of it). No separate reverse-proxy container was added; one already exists and adding a second would just be a redundant hop. |
| `kafka` | **No — opt-in** (`--profile kafka`) | See §1.1 for why this is safe to leave off. |
| `otel-collector`, `prometheus`, `loki`, `grafana` | **No — opt-in** (`--profile observability`) | Full observability stack, identical images/config to the dev compose file. |

```bash
# Minimal footprint: Postgres + API + frontend/proxy only
docker compose -f docker-compose.onprem.yml up -d

# Add Kafka
docker compose -f docker-compose.onprem.yml --profile kafka up -d

# Add observability
docker compose -f docker-compose.onprem.yml --profile observability up -d

# Both
docker compose -f docker-compose.onprem.yml --profile kafka --profile observability up -d
```

### 1.1 Why Kafka and observability are safe to leave off by default

Confirmed this session by reading `backend/src/Host/FusionOS.Api.Host/Program.cs` and the
`KafkaEventBus`/`KafkaConsumerHostedService`/`OutboxDispatcher` implementations it wires up:

- When the configured Kafka broker is unreachable, connection attempts retry and fail **silently**
  (logged, not thrown) — nothing in the request pipeline blocks on Kafka being up.
- REST APIs keep working normally with no broker present at all.
- Each module's outbox dispatcher background service just logs an error and keeps polling on its
  normal interval; the moment a broker becomes reachable (e.g. the customer later opts into the
  `kafka` profile), it starts publishing the backlog — nothing is lost, because the outbox pattern
  makes Postgres the durable source of truth (also noted in `docs/DISASTER_RECOVERY.md`).
- The OTLP trace/metric exporters and the Serilog Loki sink are all no-ops when their endpoint
  config is blank (`Observability:OtlpEndpoint` / `Observability:LokiUrl`), which is this file's
  default — see `Program.cs`.

A customer's IT team running this standalone does not need Kafka or the observability stack just to
boot the app and serve traffic. Turn them on later via the profiles above with zero data loss.

## 2. Migrations — the exact production release step

`Program.cs`'s migration bootstrap block (search for `MigrateAsync`) only runs automatically when
`ASPNETCORE_ENVIRONMENT=Development`:

```csharp
if (app.Environment.IsDevelopment())
{
    // ... await sp.GetRequiredService<...DbContext>().Database.MigrateAsync(); per module
}
```

`docker-compose.onprem.yml` deliberately sets `ASPNETCORE_ENVIRONMENT=Production`, so **this block
does not run** in an on-prem deploy — migrations must be applied as their own explicit release
step, before starting (or restarting) the `api` container against a new schema version.

Run one `dotnet ef database update` per module context, using the same `--project`/
`--startup-project`/`--context` shape `scripts/generate-migrations.sh` already uses (that script
predates the Billing module — the list below is the complete, current set read directly out of
`Program.cs`'s migration block, 16 module `DbContext`s):

```bash
HOST_PROJECT="backend/src/Host/FusionOS.Api.Host/FusionOS.Api.Host.csproj"

dotnet ef database update --project backend/src/Modules/Core/FusionOS.Modules.Core.Infrastructure/FusionOS.Modules.Core.Infrastructure.csproj                     --startup-project "$HOST_PROJECT" --context CoreDbContext
dotnet ef database update --project backend/src/Modules/Crm/FusionOS.Modules.Crm.Infrastructure/FusionOS.Modules.Crm.Infrastructure.csproj                       --startup-project "$HOST_PROJECT" --context CrmDbContext
dotnet ef database update --project backend/src/Modules/Finance/FusionOS.Modules.Finance.Infrastructure/FusionOS.Modules.Finance.Infrastructure.csproj           --startup-project "$HOST_PROJECT" --context FinanceDbContext
dotnet ef database update --project backend/src/Modules/Hrms/FusionOS.Modules.Hrms.Infrastructure/FusionOS.Modules.Hrms.Infrastructure.csproj                     --startup-project "$HOST_PROJECT" --context HrmsDbContext
dotnet ef database update --project backend/src/Modules/Inventory/FusionOS.Modules.Inventory.Infrastructure/FusionOS.Modules.Inventory.Infrastructure.csproj     --startup-project "$HOST_PROJECT" --context InventoryDbContext
dotnet ef database update --project backend/src/Modules/Warehouse/FusionOS.Modules.Warehouse.Infrastructure/FusionOS.Modules.Warehouse.Infrastructure.csproj     --startup-project "$HOST_PROJECT" --context WarehouseDbContext
dotnet ef database update --project backend/src/Modules/Procurement/FusionOS.Modules.Procurement.Infrastructure/FusionOS.Modules.Procurement.Infrastructure.csproj --startup-project "$HOST_PROJECT" --context ProcurementDbContext
dotnet ef database update --project backend/src/Modules/Sales/FusionOS.Modules.Sales.Infrastructure/FusionOS.Modules.Sales.Infrastructure.csproj                 --startup-project "$HOST_PROJECT" --context SalesDbContext
dotnet ef database update --project backend/src/Modules/Manufacturing/FusionOS.Modules.Manufacturing.Infrastructure/FusionOS.Modules.Manufacturing.Infrastructure.csproj --startup-project "$HOST_PROJECT" --context ManufacturingDbContext
dotnet ef database update --project backend/src/Modules/Quality/FusionOS.Modules.Quality.Infrastructure/FusionOS.Modules.Quality.Infrastructure.csproj           --startup-project "$HOST_PROJECT" --context QualityDbContext
dotnet ef database update --project backend/src/Modules/Maintenance/FusionOS.Modules.Maintenance.Infrastructure/FusionOS.Modules.Maintenance.Infrastructure.csproj --startup-project "$HOST_PROJECT" --context MaintenanceDbContext
dotnet ef database update --project backend/src/Modules/BusinessIntelligence/FusionOS.Modules.BusinessIntelligence.Infrastructure/FusionOS.Modules.BusinessIntelligence.Infrastructure.csproj --startup-project "$HOST_PROJECT" --context BusinessIntelligenceDbContext
dotnet ef database update --project backend/src/Modules/Ai/FusionOS.Modules.Ai.Infrastructure/FusionOS.Modules.Ai.Infrastructure.csproj                           --startup-project "$HOST_PROJECT" --context AiDbContext
dotnet ef database update --project backend/src/Modules/Marketplace/FusionOS.Modules.Marketplace.Infrastructure/FusionOS.Modules.Marketplace.Infrastructure.csproj --startup-project "$HOST_PROJECT" --context MarketplaceDbContext
dotnet ef database update --project backend/src/Modules/IntegrationHub/FusionOS.Modules.IntegrationHub.Infrastructure/FusionOS.Modules.IntegrationHub.Infrastructure.csproj --startup-project "$HOST_PROJECT" --context IntegrationHubDbContext
dotnet ef database update --project backend/src/Modules/Billing/FusionOS.Modules.Billing.Infrastructure/FusionOS.Modules.Billing.Infrastructure.csproj           --startup-project "$HOST_PROJECT" --context BillingDbContext
```

Each command needs `ConnectionStrings__Postgres` set in the shell/environment it runs in (same
variable the app itself reads — see §3), pointed at the real target database, not a dev default.
Run these **before** starting a new `api` image version against that database, and again any time
a release adds new migrations to any module.

`Program.cs`'s own comment on this block says it plainly: "a production deploy applies migrations
as its own release step, not on every app start." Treat this section as that step.

## 3. Secrets and environment configuration

`Program.cs`'s fail-fast block (search for `Missing required configuration`) runs whenever
`builder.Environment.IsDevelopment()` is false and the environment isn't `Testing` — which includes
this on-prem `Production` deploy — and throws at startup if either of these is unset:

| Config key | Env var (double-underscore convention) | What it is |
|---|---|---|
| `ConnectionStrings:Postgres` | `ConnectionStrings__Postgres` | Full Npgsql connection string every module's `DbContext` shares, e.g. `Host=postgres;Port=5432;Database=fusionos;Username=fusionos;Password=<real password>` |
| `Jwt:SigningKey` | `Jwt__SigningKey` | HMAC signing key for staff and customer-portal JWTs. Must be a real secret — `docker-compose.onprem.yml` ships an obvious placeholder (`REPLACE-ME-with-a-real-secret-before-any-non-local-deploy`) specifically so it's impossible to miss. |

ASP.NET Core's configuration binder maps `__` (double underscore) in an environment variable name
to `:` in configuration-key hierarchy — that's why `ConnectionStrings:Postgres` is set via the env
var `ConnectionStrings__Postgres`, not a literal colon (shells don't like colons in variable names).

How to actually set these for `docker-compose.onprem.yml`:

- **Preferred**: create a `.env.onprem` (gitignored — do not commit it) next to the compose file
  and reference it with `docker compose -f docker-compose.onprem.yml --env-file .env.onprem up -d`,
  or override via a `docker-compose.onprem.override.yml` that sets `environment:` on the `api`
  service to the real values.
- **Simpler / less good**: export them in the shell that runs `docker compose up` and reference
  `${ConnectionStrings__Postgres}` / `${Jwt__SigningKey}` from the compose file's `environment:`
  block instead of the placeholder literals currently there.

Either way, never commit real values to `docker-compose.onprem.yml`, `appsettings.json`, or any
tracked file — this matches `07_SECURITY.md`'s existing standard, already enforced by the fail-fast
check above.

See `docs/blueprint/14_DEPLOYMENT_GUIDE.md` §2 for the full environment-variable reference table
(Kafka, SendGrid, CORS, rate limiting, Integration Hub connector credentials, etc.) — not repeated
here since none of those are on-prem-specific.

## 4. Backup and restore

Full detail, scope, and what's still missing lives in `docs/DISASTER_RECOVERY.md` — this section
only adds the on-prem-specific restore-into-a-new-environment step and the measured drill results.

### 4.1 Routine backup (production)

```bash
FUSIONOS_PG_HOST=<postgres host> \
FUSIONOS_PG_PORT=5432 \
FUSIONOS_PG_USER=fusionos \
FUSIONOS_PG_DATABASE=fusionos \
FUSIONOS_PG_PASSWORD=<real password> \
  ./scripts/backup-postgres.sh
```

Writes a timestamped, compressed `pg_dump --format=custom` file to `backups/` (gitignored) and
prunes anything past the retention window (`KEEP_BACKUPS`, default 14). Wire this into whatever
scheduler the on-prem host actually has available — cron, Windows Task Scheduler, a Kubernetes
CronJob — there is no scheduler built into this repo or triggered by either compose file.

### 4.2 Restore into a fresh environment (e.g. standing up a new on-prem box from a backup)

```bash
createdb -h <postgres host> -U fusionos fusionos
FUSIONOS_PG_HOST=<postgres host> FUSIONOS_PG_DATABASE=fusionos \
  ./scripts/restore-postgres.sh backups/fusionos-<timestamp>.dump
```

Then apply the migration commands in §2 if the dump predates a migration that's since shipped
(a dump already contains its own `__ef_migrations_history` rows per schema, so re-running `dotnet
ef database update` against a freshly-restored dump is a no-op unless newer migrations exist), and
start the `api`/`web` services against it.

### 4.3 Drill actually run this session (2026-08-04)

Executed directly against the local dev Postgres (Windows PostgreSQL 18 service, native install,
`localhost:5432`, db `fusionos`, user/password `fusionos`/`fusionos` — not Docker in this
environment, so `pg_dump`/`pg_restore`/`createdb`/`dropdb` were invoked directly from
`C:\Program Files\PostgreSQL\18\bin\` with the same flags `scripts/backup-postgres.sh` and
`scripts/restore-postgres.sh` use, rather than editing either script to assume a container name that
doesn't exist here):

| Step | Command (effective) | Result |
|---|---|---|
| Backup | `pg_dump --host=localhost --port=5432 --username=fusionos --dbname=fusionos --format=custom --compress=9 --file=backups/fusionos-20260804T104823Z.dump` | **0.693s**, 534,583 bytes |
| Create test DB | `createdb --host=localhost --port=5432 --username=fusionos fusionos_restore_test` | ok |
| Restore | `pg_restore --host=localhost --port=5432 --username=fusionos --dbname=fusionos_restore_test --clean --if-exists --no-owner backups/fusionos-20260804T104823Z.dump` | **2.963s** |
| Verify: total tables | `information_schema.tables` count, both DBs, all 16 schemas (`ai`, `bi`, `billing`, `core`, `crm`, `finance`, `hrms`, `integration_hub`, `inventory`, `maintenance`, `manufacturing`, `marketplace`, `procurement`, `quality`, `sales`, `warehouse`) | **202 / 202** — identical per-schema counts in both `fusionos` and `fusionos_restore_test` |
| Verify: row counts | `core.companies`, `inventory.products`, `finance.journal_entries` | `core.companies` 5/5, `inventory.products` 42/42, `finance.journal_entries` 4/4 — exact match, source vs. restored |
| Cleanup | `dropdb --host=localhost --port=5432 --username=fusionos fusionos_restore_test` | done — confirmed only `fusionos` remains (`select datname from pg_database where datname like 'fusionos%'` returned one row) |

The live `fusionos` database was never written to or altered at any point — `pg_dump` only reads
it, and every restore/verify/drop step targeted `fusionos_restore_test` exclusively.

### 4.4 RTO / RPO

- **RTO (measured, this drill, this data volume):** backup 0.693s + restore 2.963s = **~3.66s**
  total mechanical time, plus however long it takes to point a new/recovered `api` host at the
  restored database (not separately measured here — no app container was started against
  `fusionos_restore_test` in this drill). This number is only valid at the current data volume
  (202 tables, low row counts across the board — this is a dev/demo dataset, not a production-scale
  one); re-measure before relying on it against real production data volume, per
  `docs/DISASTER_RECOVERY.md`'s own caveat.
- **RPO (assumption, not measured):** no scheduler exists anywhere in this repo or either compose
  file to run `backup-postgres.sh` automatically (confirmed: no cron reference, no GitHub Actions
  cron job, no Kubernetes CronJob manifest checked in). Until an on-prem deployment wires the backup
  script into an actual scheduler (§4.1), there is no real RPO — only whatever interval a human
  remembers to run it at manually. **Assumption, to be confirmed with whoever owns the on-prem
  deployment**: a daily backup cadence (once every 24h) is a reasonable starting point given the
  measured backup cost is sub-second at current volume and likely stays well under a minute even at
  meaningfully larger production volume; tighten to hourly if the customer's actual change rate
  justifies it. This is a stated assumption, not a measurement — update it once a real schedule
  exists.
