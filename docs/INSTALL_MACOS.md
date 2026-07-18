# FusionOS — Installation Guide (macOS)

This walks through getting FusionOS running locally on macOS (Intel or Apple Silicon), from a
clean machine to a working API + frontend against a real Postgres database. See `README.md` for
architecture and `docs/blueprint/` for the standards every module follows.

## 1. Prerequisites

Install these once. [Homebrew](https://brew.sh) is the easiest path for all of them:

```bash
# Homebrew, if you don't already have it
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```

| Tool | Why | Install |
|---|---|---|
| **.NET 8 SDK** | builds/runs the backend, `dotnet ef` migrations | `brew install --cask dotnet-sdk` or https://dotnet.microsoft.com/download/dotnet/8.0 |
| **Docker Desktop** | Postgres, Redis, Kafka, observability stack via `docker-compose.yml` | `brew install --cask docker` or https://www.docker.com/products/docker-desktop/ |
| **Node.js 20+** | frontend build/dev server | `brew install node@20` or https://nodejs.org |
| **Git** | clone/manage the repo | `brew install git` (Xcode Command Line Tools also provide this) |

Verify each:

```bash
dotnet --version        # should print 8.0.x
docker --version
node --version           # should print v20.x or later
```

Launch Docker Desktop from Applications and wait for the whale icon in the menu bar to show
"Docker Desktop is running" before continuing.

One more one-time tool, after the SDK is installed:

```bash
dotnet tool install --global dotnet-ef
```

If your shell doesn't find `dotnet-ef` afterward, add dotnet global tools to your `PATH` (the
installer prints the exact line — typically `export PATH="$PATH:$HOME/.dotnet/tools"` in your
`~/.zshrc` or `~/.bash_profile`), then open a new terminal tab.

## 2. Clone and configure

```bash
git clone <your-fork-or-repo-url> FusionOS
cd FusionOS
cp frontend/.env.example frontend/.env
```

Backend secrets (`Jwt:SigningKey`, `ConnectionStrings:Postgres`) are read from configuration and
**fail fast on startup outside Development** if unset — for local dev,
`appsettings.Development.json` already has working defaults matching `docker-compose.yml`'s
Postgres credentials (`fusionos`/`fusionos`/`fusionos` db/user/password on `localhost:5432`).
Don't reuse those values for anything beyond local development.

## 3. Start infrastructure

From the repo root:

```bash
docker compose up -d postgres redis kafka
```

Wait ~10-15 seconds for Postgres's healthcheck to pass. Check with:

```bash
docker compose ps
```

Optionally bring up the observability stack too (not required to run the app):

```bash
docker compose up -d otel-collector prometheus loki grafana
```

Grafana is then at http://localhost:3000 (`admin`/`admin`), with Loki and Prometheus datasources
already provisioned.

## 4. Generate and apply database migrations

**This is the step nothing in this repo has ever had run against it for real — treat any failure
here as the top-priority thing to fix.**

```bash
chmod +x scripts/generate-migrations.sh   # first time only
./scripts/generate-migrations.sh
```

This generates the first `InitialCreate` migration for every module that has real EF
configurations (Core, Inventory, Warehouse, Procurement, Sales, Finance, Manufacturing, Crm,
Quality). Review what it generated under each module's `Persistence/Migrations` folder, then
apply it:

```bash
./scripts/generate-migrations.sh --apply
```

If a module fails with an EF configuration error, that's real signal — fix the reported issue in
that module's `*.Infrastructure` project and re-run (delete the partially-generated
`Persistence/Migrations` folder for that module first if `add` didn't complete cleanly).

## 5. Run the backend

From `backend/`:

```bash
cd backend
dotnet build FusionOS.sln
dotnet run --project src/Host/FusionOS.Api.Host
```

`dotnet build` first is deliberate — this surfaces any compile error before `dotnet run` masks it
behind a slower build-then-run cycle. The API listens on the port printed in the console
(check `src/Host/FusionOS.Api.Host/Properties/launchSettings.json` if you need the exact URL).

## 6. Run the frontend

In a **separate terminal tab**, from the repo root:

```bash
cd frontend
npm install
npm run dev
```

Open the URL Vite prints (typically http://localhost:5173).

> **Apple Silicon note:** this frontend's Vite build (`rolldown-vite`) uses a native binary
> (`@rolldown/binding-darwin-arm64`). If `npm install` doesn't fetch the right one automatically,
> delete `node_modules` and `package-lock.json` and reinstall — do not force an x86 binary via
> Rosetta unless you have a specific reason to.

## 7. Smoke-test the app

1. Register a company (`POST /api/v1/core/companies` is anonymous — this is the tenant bootstrap
   action) via the frontend's signup flow.
2. Register the company's first (Owner) user, then log in.
3. Walk one real flow per module to confirm the stack is wired end to end, e.g.:
   - Create a Supplier → Purchase Order → Goods Receipt → check the Inventory ledger entry posted.
   - Create a Sales Invoice → check the AR ledger entry posted.
   - Create a Cost Center → Budget → run a vs-actual report.

## 8. Run the test suites

```bash
cd backend
dotnet test

cd ../frontend
npm test
npm run test:e2e   # first time only: npx playwright install
```

## Alternative: fully containerized

Skip steps 5-6 and instead, after step 4's migrations are applied at least once:

```bash
docker compose up --build
```

This builds and runs both the API (`http://localhost:5000`) and frontend
(`http://localhost:5173`) as containers. On Apple Silicon, Docker Desktop pulls
`arm64`-compatible images for `postgres`/`redis`; `apache/kafka:3.7.0` also ships multi-arch, so no
emulation should be needed.

## Troubleshooting

- **`docker compose` says "Cannot connect to the Docker daemon"** — Docker Desktop isn't running
  yet; open it from Applications and wait for the whale icon to settle.
- **Port already in use** (`5432`, `6379`, `9092`, `5000`, `5173`) — something else on your machine
  (e.g. a Homebrew-installed Postgres running as a service) is already bound to that port. Either
  stop it (`brew services stop postgresql`) or edit the port mapping in `docker-compose.yml`.
- **`dotnet ef` not found after install** — your shell's `PATH` doesn't include
  `~/.dotnet/tools` yet; add it to your shell profile (see step 1) and open a new terminal.
- **Gatekeeper blocks a downloaded binary** — if macOS refuses to run something you installed
  outside Homebrew/the App Store, System Settings → Privacy & Security has an "Allow Anyway"
  prompt after the first blocked attempt.
