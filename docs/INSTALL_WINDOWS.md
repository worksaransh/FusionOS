# FusionOS — Installation Guide (Windows)

This walks through getting FusionOS running locally on Windows 10/11, from a clean machine to a
working API + frontend against a real Postgres database. See `README.md` for architecture and
`docs/blueprint/` for the standards every module follows.

## 1. Prerequisites

Install these once, in order. Use **Windows PowerShell 5.1 or PowerShell 7+** for every command
below.

| Tool | Why | Install |
|---|---|---|
| **.NET 8 SDK** | builds/runs the backend, `dotnet ef` migrations | `winget install Microsoft.DotNet.SDK.8` or https://dotnet.microsoft.com/download/dotnet/8.0 |
| **Docker Desktop** | Postgres, Redis, Kafka, observability stack via `docker-compose.yml` | `winget install Docker.DockerDesktop` or https://www.docker.com/products/docker-desktop/ (requires WSL2 — Docker Desktop's installer will prompt to enable it) |
| **Node.js 20+** | frontend build/dev server | `winget install OpenJS.NodeJS.LTS` or https://nodejs.org |
| **Git** | clone/manage the repo | `winget install Git.Git` (skip if already installed) |

After installing, **restart your terminal** (PATH changes don't apply to already-open shells), then verify:

```powershell
dotnet --version        # should print 8.0.x
docker --version
node --version           # should print v20.x or later
```

Start Docker Desktop from the Start menu and wait for it to say "Docker Desktop is running"
before continuing — the `docker` CLI will fail with a connection error otherwise.

One more one-time tool, after the SDK is installed:

```powershell
dotnet tool install --global dotnet-ef
```

If PowerShell says `dotnet-ef` isn't recognized right after installing it, open a new terminal —
global tools are added to a PATH entry that only applies to new shells.

## 2. Clone and configure

```powershell
git clone <your-fork-or-repo-url> FusionOS
cd FusionOS
copy frontend\.env.example frontend\.env
```

Backend secrets (`Jwt:SigningKey`, `ConnectionStrings:Postgres`) are read from configuration and
**fail fast on startup outside Development** if unset — for local dev, `appsettings.Development.json`
already has working defaults matching `docker-compose.yml`'s Postgres credentials
(`fusionos`/`fusionos`/`fusionos` db/user/password on `localhost:5432`). Don't reuse those values
for anything beyond local development.

## 3. Start infrastructure

From the repo root:

```powershell
docker compose up -d postgres redis kafka
```

Wait ~10-15 seconds for Postgres's healthcheck to pass. Check with:

```powershell
docker compose ps
```

Optionally bring up the observability stack too (not required to run the app):

```powershell
docker compose up -d otel-collector prometheus loki grafana
```

Grafana is then at http://localhost:3000 (`admin`/`admin`), with Loki and Prometheus datasources
already provisioned.

## 4. Generate and apply database migrations

**This is the step nothing in this repo has ever had run against it for real — treat any failure
here as the top-priority thing to fix.**

```powershell
.\scripts\generate-migrations.ps1
```

This generates the first `InitialCreate` migration for every module that has real EF
configurations (Core, Inventory, Warehouse, Procurement, Sales, Finance, plus whichever of the
Phase-F scaffold modules have configurations by the time you run this). Review what it generated
under each module's `Persistence/Migrations` folder, then apply it:

```powershell
.\scripts\generate-migrations.ps1 -Apply
```

If a module fails with an EF configuration error, that's real signal — fix the reported issue in
that module's `*.Infrastructure` project and re-run (delete the partially-generated
`Persistence/Migrations` folder for that module first if `add` didn't complete cleanly).

## 5. Run the backend

From `backend/`:

```powershell
cd backend
dotnet build FusionOS.sln
dotnet run --project src\Host\FusionOS.Api.Host
```

`dotnet build` first is deliberate — this surfaces any compile error before `dotnet run` masks it
behind a slower build-then-run cycle. The API listens on the port printed in the console
(check `src\Host\FusionOS.Api.Host\Properties\launchSettings.json` if you need the exact URL).

## 6. Run the frontend

In a **separate terminal**, from the repo root:

```powershell
cd frontend
npm install
npm run dev
```

Open the URL Vite prints (typically http://localhost:5173).

## 7. Smoke-test the app

1. Register a company (`POST /api/v1/core/companies` is anonymous — this is the tenant bootstrap
   action) via the frontend's signup flow.
2. Register the company's first (Owner) user, then log in.
3. Walk one real flow per module to confirm the stack is wired end to end, e.g.:
   - Create a Supplier → Purchase Order → Goods Receipt → check the Inventory ledger entry posted.
   - Create a Sales Invoice → check the AR ledger entry posted.
   - Create a Cost Center → Budget → run a vs-actual report.

## 8. Run the test suites

```powershell
cd backend
dotnet test

cd ..\frontend
npm test
npm run test:e2e   # first time only: npx playwright install
```

## Alternative: fully containerized

Skip steps 5-6 and instead, after step 4's migrations are applied at least once:

```powershell
docker compose up --build
```

This builds and runs both the API (`http://localhost:5000`) and frontend
(`http://localhost:5173`) as containers.

## Troubleshooting

- **`docker` commands hang or refuse to connect** — Docker Desktop isn't fully started yet, or
  WSL2 needs a restart (`wsl --shutdown` from any terminal, then reopen Docker Desktop).
- **PowerShell doesn't support `&&`** — Windows PowerShell 5.1 (not PowerShell 7+) doesn't support
  chaining commands with `&&`. Run commands on separate lines, or use `;` instead.
- **Port already in use** (`5432`, `6379`, `9092`, `5000`, `5173`) — something else on your machine
  is already bound to that port; stop it or edit the port mapping in `docker-compose.yml`.
- **`dotnet ef` not found after install** — open a new terminal; global tool PATH entries don't
  apply retroactively to already-open shells.
