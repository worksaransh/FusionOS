# 14_DEPLOYMENT_GUIDE.md — Deployment Guide

This document is a snapshot of what actually exists in this repo for running FusionOS — locally
and (with caveats spelled out below) in production. Per this codebase's own established voice
(see `docs/blueprint/12_AI_PLATFORM.md` and `docs/PROJECT_TRACKER.md`), it names real gaps instead
of describing capability the code doesn't have. Nothing here was fabricated to look more
production-ready than the code actually is.

## 1. Local development

### 1.1 What `docker-compose.yml` actually starts

Reading `docker-compose.yml` at the repo root, `docker compose up` (or a scoped subset) brings up:

| Service | Image | Purpose |
|---|---|---|
| `postgres` | `postgres:16-alpine` | The single Postgres instance every module's schema lives in (`fusionos` database, `fusionos`/`fusionos` user/password — dev-only, see §2) |
| `redis` | `redis:7-alpine` | Caching (referenced by the stack; see `02_TECH_STACK.md`) |
| `kafka` | `apache/kafka:3.7.0` (KRaft mode, no separate Zookeeper) | Cross-module integration events — outbox → Kafka → consumer, per `03_SYSTEM_ARCHITECTURE.md` §4 |
| `otel-collector` | `otel/opentelemetry-collector-contrib:0.102.0` | Receives OTLP traces/metrics from the API |
| `prometheus` | `prom/prometheus:v2.53.0` | Scrapes the API's `/metrics` endpoint |
| `loki` | `grafana/loki:2.9.8` | Log aggregation (API logs ship here via Serilog when configured) |
| `grafana` | `grafana/grafana:11.1.0` | Dashboards over Loki + Prometheus (`admin`/`admin`, both datasources auto-provisioned from `observability/grafana/provisioning`) |
| `api` | built from `backend/Dockerfile` | The .NET 8 API host, all modules in one process |
| `web` | built from `frontend/Dockerfile` | The React frontend, built and served via nginx |

Named volumes persist Postgres/Prometheus/Loki/Grafana data across restarts
(`fusionos_postgres_data`, `fusionos_prometheus_data`, `fusionos_loki_data`,
`fusionos_grafana_data`).

Typical invocations (from the compose file's own header comment):

```
docker compose up -d postgres redis kafka                      # infra only, run backend/frontend locally with `dotnet run`/`npm run dev`
docker compose up --build                                      # everything, containerized
docker compose up -d otel-collector prometheus loki grafana    # observability only
```

The `api` service's `environment:` block in `docker-compose.yml` already sets
`ConnectionStrings__Postgres`, `Kafka__BootstrapServers`, `ASPNETCORE_ENVIRONMENT=Development`,
`Observability__OtlpEndpoint`, and `Observability__LokiUrl` for you — pointed at the other
compose services by their service names (`postgres`, `kafka`, `otel-collector`, `loki`). Nothing
else needs to be set to bring the containerized stack up as-is. The `web` service needs no
environment variables at all; it's a static build served by nginx (see `frontend/Dockerfile` and
§1.3).

### 1.2 Secrets/config a developer must set before running the app *without* the `api` container

If you run the backend directly (`dotnet run`) instead of via `docker compose up --build`, none
of the `api` service's `environment:` values apply, so you fall back to
`appsettings.json`/`appsettings.Development.json` and whatever you export yourself. Reading
`backend/src/Host/FusionOS.Api.Host/Program.cs` and both `appsettings*.json` files directly:

- **`ConnectionStrings:Postgres`** — `appsettings.Development.json` already supplies a working
  default (`Host=localhost;Port=5432;...`) pointed at the compose Postgres service exposed on
  `localhost:5432`. In `Development` this fails silently open (empty string is fine); **outside
  Development, `Program.cs` throws `InvalidOperationException` at startup if this is unset** — see
  the fail-fast block near the top of `Program.cs`.
- **`Jwt:SigningKey`** — same fail-fast treatment. `appsettings.Development.json` supplies
  `development-only-signing-key-do-not-use-in-any-real-environment` (also hardcoded as a fallback
  literal directly in `Program.cs` if the config key is blank in Development) — **never use this
  value outside Development.**
- Everything else in `appsettings.json` (`Kafka:BootstrapServers`, `SendGrid:*`, `Cors:*`,
  `IpRateLimiting:*`, `Observability:*`) has a working-enough default or blank-is-safe behavior in
  Development — see the full reference table in §2.

Practical minimum to run `dotnet run --project src/Host/FusionOS.Api.Host` against a
`docker compose up -d postgres redis kafka` infra set: nothing extra needs to be exported: the
committed `appsettings.Development.json` is already enough. Setting real values only matters once
you leave `ASPNETCORE_ENVIRONMENT=Development`.

### 1.3 The exact commands (and where they stop being honest)

```
# 1. Infra
docker compose up -d postgres redis kafka

# 2. Database schema — see the "migrations" callout below before running this
./scripts/generate-migrations.sh
./scripts/generate-migrations.sh --apply   # or the `dotnet ef database update` commands it prints

# 3. Backend
cd backend
dotnet run --project src/Host/FusionOS.Api.Host

# 4. Frontend (separate terminal)
cd frontend
npm install
npm run dev
```

**Migrations — the single biggest honesty gap in this whole guide.** A repo-wide search for
`Microsoft.EntityFrameworkCore.Migrations` across `backend/` returns **zero matches**. There is no
`Migrations/` folder anywhere containing an actual EF Core migration class for any module's
`DbContext` (the `Migrations/` folders that do exist — under
`backend/src/Modules/IntegrationHub/.../Migrations/` — are that module's own business feature, a
legacy-system-migration-job pipeline with `MigrationJob`/`MigrationStagingRecord` entities; they
have nothing to do with EF Core schema migrations, despite the folder name collision). `docs/
PROJECT_TRACKER.md` documents this as the standing, unresolved "Phase G" blocker: this environment
has never had a .NET SDK available to run `dotnet ef migrations add`, so `scripts/
generate-migrations.sh` has never actually been executed against a real Postgres instance. **A
real deployment cannot create its database schema from this repo as it stands today** — generating
the first migration for every module's `DbContext` and applying it is a real, unstarted
prerequisite, not a formality. Do this before anything else in a genuinely new environment; do not
assume `./scripts/generate-migrations.sh` "just works" because it exists and looks complete.

### 1.4 Frontend tests

```
cd frontend
npm test           # Vitest + React Testing Library
npm run test:e2e   # Playwright — needs `npx playwright install` once
```

Whether these commands are *runnable* has genuinely differed by environment across this project's
history (see `README.md`'s "Verification in this environment" section) — earlier rounds hit a
native-binary segfault in `rolldown-vite` that blocked Vitest/Playwright/`vite build` entirely, and
had to fall back to `tsc -b --force` as the only real check. On a normal Windows/macOS/Linux
developer machine (confirmed directly on a Windows machine during this Phase 9 pass) none of that
applies: `npx vite --version` runs cleanly, and Playwright's Chromium build installs and runs real
tests. Don't assume the older segfault note in `README.md` describes your machine — check `npx
vite --version` yourself.

## 2. Environment variables reference

Compiled by reading every module's `RegisterServices` (`*Module.cs`), `Program.cs`, and both
`appsettings*.json` files under `backend/src/Host/FusionOS.Api.Host/`. Every module wires its own
`DbContext` against the same `ConnectionStrings:Postgres` value (each with its own Postgres schema
and its own `__ef_migrations_history` table, e.g. `core`, `finance` — see each `*Module.cs`'s
`MigrationsHistoryTable(...)` call) — no module reads a config key of its own beyond that shared
connection string, so there is one connection-string row below, not one per module.

| Key (double-underscore for env vars, e.g. `Jwt__SigningKey`) | Purpose | Required? |
|---|---|---|
| `ConnectionStrings:Postgres` | Postgres connection string, shared by every module's `DbContext` | **Required outside Development** — `Program.cs` throws at startup if blank. Dev default supplied in `appsettings.Development.json`. |
| `Jwt:SigningKey` | HMAC signing key for staff *and* customer-portal JWTs (`Sales`'s `PortalJwtTokenService` reuses the same key/issuer/audience — told apart by claims, not a second scheme) | **Required outside Development.** Dev-only fallback literal exists in `Program.cs`; never use it outside Development. |
| `Jwt:Issuer` | JWT issuer claim | Optional — defaults to `"fusionos"` in code if unset. |
| `Jwt:Audience` | JWT audience claim | Optional — defaults to `"fusionos-clients"` in code if unset. |
| `Kafka:BootstrapServers` | Kafka broker address(es) for the outbox dispatcher + consumers | Optional in the sense that it has a `localhost:9092` default in `appsettings.json`, but the app needs a real broker reachable at that address for any cross-module event to actually flow. |
| `Kafka:TopicPrefix` | Prefixes every Kafka topic name | Optional — defaults to `"fusionos"`. |
| `SendGrid:ApiKey` | SendGrid API key for Notification delivery (`SendGridNotificationSender`) | Optional — blank by design (`docs/PROJECT_TRACKER.md` Phase M7). A blank key makes delivery attempts fail visibly (logged, `Notification.MarkDeliveryFailed`) without crashing the app; the in-app notification inbox keeps working regardless. |
| `SendGrid:FromAddress` / `SendGrid:FromName` | Sender identity for outbound notification emails | Optional — defaults supplied in `appsettings.json` (`notifications@fusionos.local` / `FusionOS`). |
| `Cors:AllowedOrigins` | Array of origins allowed by the `Default` CORS policy | Optional — empty array by default (no origins allowed at all outside what you configure); `appsettings.Development.json` adds `http://localhost:5173` for local frontend dev. |
| `IpRateLimiting:*` | `AspNetCoreRateLimit` configuration — global per-IP caps plus tight per-minute limits on the three unauthenticated Auth endpoints | Optional — a complete default policy ships in `appsettings.json`; override only to change the actual limits. |
| `Observability:OtlpEndpoint` | OTLP gRPC endpoint traces/metrics export to (the `otel-collector` service in `docker-compose.yml`) | Optional — blank by default; when blank, traces/metrics are only visible via the Development console exporter and the `/metrics` Prometheus scrape endpoint, not shipped anywhere. |
| `Observability:LokiUrl` | Grafana Loki push endpoint for Serilog's Loki sink | Optional — blank by default; the sink degrades to a harmless self-log warning (not a crash) if unreachable or unset. |
| `AllowedHosts` | ASP.NET Core host-header allow-list | Optional — `"*"` by default. |
| **`{CredentialReference}_...` — Integration Hub connector credentials** | See below | Optional, per-connector, only if that connector is actually used |

### Integration Hub's `{CredentialReference}_...` convention

Phase 8's `ConnectorConnection.ConfigureCredential(string credentialReference)` deliberately stores
only an opaque *reference string* on the connection row — never a raw secret — and each
`IConnectorSyncAdapter` implementation resolves the real secret out of `IConfiguration` at call
time only, using that reference as a prefix. Reading each adapter directly
(`backend/src/Modules/IntegrationHub/FusionOS.Modules.IntegrationHub.Infrastructure/Sync/`):

| Connector | Config key(s) read (where `{ref}` = the `CredentialReference` you configured on the connection) |
|---|---|
| Stripe | `{ref}_SECRET_KEY` |
| Shopify | `{ref}` itself (the Admin API access token) **and** `{ref}_SHOP_DOMAIN` |
| Razorpay | `{ref}_KEY_ID` and `{ref}_KEY_SECRET` |
| Delhivery | `{ref}_API_TOKEN` |
| Shiprocket | `{ref}_EMAIL` and `{ref}_PASSWORD` |

These resolve through the same `IConfiguration` as everything else, so they can be set as ordinary
environment variables (e.g. `MYSHOPIFYSTORE_SHOP_DOMAIN=mystore.myshopify.com`) or under a
`ConnectorCredentials` section in `appsettings.json` if you prefer a file — nothing in the code
requires environment variables specifically. **This is exactly as far as Phase 8's design goes**:
there is no secrets-manager/vault integration anywhere in this repo (see §3) — "store a reference,
resolve a raw environment variable at call time" is the entire credential-storage story today, by
explicit design, not an oversight.

## 3. Production considerations — an honest gap list

Matching this codebase's own established practice of naming what's missing rather than describing
aspirational capability (see `docs/PROJECT_TRACKER.md`'s Phase G framing and
`docs/blueprint/12_AI_PLATFORM.md`'s explicit scope-outs), here is what a real production
deployment would need that nothing in this repo currently provides:

- **No migration-generation-and-apply pipeline has ever been exercised.** Covered in depth in
  §1.3 — zero EF Core migrations exist for any module. `scripts/generate-migrations.sh` is
  written but has never run against a real Postgres instance in this project's history (no .NET
  SDK has ever been available in any environment this repo has been built in, including this
  Phase 9 pass — `dotnet --version` on this pass's own machine reports "No .NET SDKs were found").
  This is the single highest-priority gap before any real deployment, per `docs/PROJECT_TRACKER.md`
  Section 7.
- **No secrets manager / vault integration.** `Jwt:SigningKey`, `ConnectionStrings:Postgres`,
  `SendGrid:ApiKey`, and every Integration Hub `{CredentialReference}_...` value are all read as
  plain configuration/environment variables — there is no Azure Key Vault, AWS Secrets Manager,
  HashiCorp Vault, or equivalent wired in anywhere. `07_SECURITY.md`'s standard ("never commit real
  values, set via environment variable or secret manager") is honored by *not shipping* default
  secrets, not by actually integrating with a secret manager. A real production deployment needs to
  supply these via whatever secret-injection mechanism its host platform offers (Kubernetes
  Secrets, an App Service Key Vault reference, etc.) — this repo does not do that wiring for you.
- **Health checks exist but are shallow.** `Program.cs` registers ASP.NET Core's built-in
  `AddHealthChecks()`/`MapHealthChecks("/health")`, which is a real (if minimal) liveness check —
  it confirms the process is up and responding, nothing about database/Kafka connectivity, since no
  `AddNpgSql`/`AddKafka` health check packages are registered. Separately, **every module's own
  `Xxx­HealthController` (`GET /api/v1/{module}/health`) is not a health check in any operational
  sense** — reading a representative one
  (`Manufacturing/FusionOS.Modules.Manufacturing.Api/Controllers/ManufacturingHealthController.cs`),
  it unconditionally returns a static `200 OK` with `{ module, status: "active", roadmapPhase,
  slices }` — no database ping, no dependency check, nothing that could ever report unhealthy. It
  exists to prove the module is registered and routed, not as a readiness/liveness probe a load
  balancer or orchestrator should point at. **Do not wire a Kubernetes readiness probe to any
  `*HealthController` endpoint expecting it to reflect real health** — point it at `/health`
  instead, and even that only checks process liveness, not dependency health, until `AddNpgSql`/
  Kafka health checks are actually added.
- **No load-balancing, horizontal-scaling, or high-availability configuration exists anywhere in
  this repo.** `docker-compose.yml` runs exactly one instance of every service; there is no
  Kubernetes manifest, Helm chart, or multi-replica orchestration of any kind checked into this
  repo. Nothing here should be read as implying the app is horizontally scalable, session-affinity-
  safe, or HA-ready — that claim isn't supported by anything that exists in the codebase today.
- **CD stops at "images are published," not "images are deployed."** `.github/workflows/cd.yml`
  builds and pushes both Docker images to GitHub Container Registry (GHCR) on every merge to
  `main`, tagged with the commit SHA and `latest` — its own header comment says plainly that there
  is no live environment yet for it to deploy into. There is no staging/production environment
  definition, no Kubernetes/ECS/App Service deploy step, and no rollback automation anywhere in
  this repo.
- **Backup/DR scripts exist but are unverified.** `scripts/backup-postgres.sh`/
  `restore-postgres.sh` and `docs/DISASTER_RECOVERY.md` exist and describe a real
  `pg_dump --format=custom` backup + restore flow, but per that document's own text, nobody has
  ever run a restore drill — treat any RPO/RTO numbers there as aspirational until someone does.
- **Observability ships data nowhere durable by default.** The OTLP/Loki exporters are wired but
  point at nothing (`Observability:OtlpEndpoint`/`Observability:LokiUrl` are blank by default) —
  `docker-compose.yml`'s own `otel-collector` config (`observability/otel-collector-config.yaml`)
  uses the `debug` exporter, i.e. traces/metrics are logged and discarded, not forwarded to a real
  backend like Tempo/Grafana Cloud/Datadog. A real deployment needs to point this at something
  that actually retains the data.

## 4. CI note

CI **does** exist in this repo: `.github/workflows/ci.yml` runs on every push/PR to `main` and
covers, in order: backend build + test (against a real ephemeral Postgres service container,
`dotnet restore`/`build`/`test`), frontend lint + type-check + unit test (`npm run lint`, `npm
test`), a frontend production build (`npm run build`), a Playwright E2E smoke job (installs
Chromium, runs `npm run test:e2e`), and a Docker image build for both `backend/Dockerfile` and
`frontend/Dockerfile`. `.github/workflows/cd.yml` runs on every merge to `main` and publishes both
images to GHCR (see §3 for what it deliberately does not do). `.github/dependabot.yml` runs weekly
dependency updates across NuGet, npm, Docker base images, and the GitHub Actions themselves.

**The one honest caveat**: this CI has never actually been run and observed to pass in *this*
project's own working history, because every environment this repo has been built in so far lacked
a usable `dotnet` toolchain locally (see §1.3/§3) — the backend job's real-world pass/fail status
on the current codebase has never been confirmed by anyone building this repo, only by static
review. If you're reading this on a machine with the .NET SDK, running the workflow (or just
`dotnet build`/`dotnet test` locally) for the first time is the single most valuable thing you can
do to move this repo's confidence forward — see `docs/PROJECT_TRACKER.md`'s "Phase G" for why this
is called out repeatedly as the top-priority next step.
