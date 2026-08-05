<div align="center">

  <img src="assets/banner/fusionos-hero-banner.svg" alt="FusionOS Hero Banner" width="100%" />

  <br /><br />

  <img src="assets/logo/fusionos-logo.svg" alt="FusionOS Logo" width="360" />

  <h3>The Open-Source AI-First Enterprise ERP & Operating System</h3>

  <p align="center">
    A production-ready, ultra-scalable enterprise platform built with <b>.NET 8 (Modular Monolith)</b>, <b>React 18</b>, <b>TypeScript</b>, <b>PostgreSQL</b>, <b>Kafka</b>, <b>Redis</b>, and <b>OpenTelemetry</b>.
  </p>

  <p align="center">
    <a href="#key-features">Features</a> •
    <a href="#architecture">Architecture</a> •
    <a href="#module-overview">Modules</a> •
    <a href="#installation-guide">Installation</a> •
    <a href="#download-fusionos">Downloads</a> •
    <a href="#tech-stack">Tech Stack</a> •
    <a href="#api-documentation">API Docs</a> •
    <a href="#security--compliance">Security</a>
  </p>

  <p align="center">
    <a href="https://github.com/worksaransh/FusionOS/releases"><img src="https://img.shields.io/badge/version-v1.0.0-blue.svg?style=for-the-badge&logo=git" alt="Version 1.0.0" /></a>
    <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-green.svg?style=for-the-badge&logo=open-source-initiative" alt="MIT License" /></a>
    <img src="https://img.shields.io/badge/.NET-8.0-512BD4.svg?style=for-the-badge&logo=dotnet" alt=".NET 8" />
    <img src="https://img.shields.io/badge/React-18.3-61DAFB.svg?style=for-the-badge&logo=react" alt="React 18" />
    <img src="https://img.shields.io/badge/PostgreSQL-16.0-4169E1.svg?style=for-the-badge&logo=postgresql" alt="PostgreSQL" />
    <img src="https://img.shields.io/badge/Docker-Ready-2496ED.svg?style=for-the-badge&logo=docker" alt="Docker" />
    <img src="https://img.shields.io/badge/Platform-Windows%20%7C%20macOS%20%7C%20Linux%20%7C%20PWA-lightgrey.svg?style=for-the-badge" alt="Platforms" />
  </p>

</div>

---

## 📋 Table of Contents

- [Executive Summary](#-executive-summary)
- [🖼 Screenshots](#-screenshots)
- [🏗 Architecture & Design Principles](#-architecture--design-principles)
- [📦 Enterprise Module Overview (16 Modules)](#-enterprise-module-overview-16-modules)
- [⚙️ Tech Stack Matrix](#%EF%B8%8F-tech-stack-matrix)
- [📂 Directory Structure](#-directory-structure)
- [🚀 Installation Guide](#-installation-guide)
- [⬇️ Download FusionOS](#%EF%B8%8F-download-fusionos)
- [🔑 Environment Variables Reference](#-environment-variables-reference)
- [📡 API Documentation](#-api-documentation)
- [🗄 Database & Data Architecture](#-database--data-architecture)
- [🚀 Deployment Guide](#-deployment-guide)
- [🔐 Security & Compliance](#-security--compliance)
- [🔧 Troubleshooting & FAQ](#-troubleshooting--faq)
- [🤝 Contributing & Community](#-contributing--community)
- [📄 License](#-license)

---

## 💡 Executive Summary

### What is FusionOS?

**FusionOS** is an open-source Enterprise Resource Planning (ERP) platform and business operating system for mid-market and enterprise organizations. It unifies core business functions — **Financial Accounting**, **Supply Chain**, **Manufacturing (MRP)**, **CRM**, **HRMS**, **Quality Assurance**, **Billing/Subscriptions**, and **AI-assisted operations** — into a single, cohesive platform.

Built as a **.NET 8 Modular Monolith** paired with a **React 18 SPA**, FusionOS aims to reduce the fragmentation, licensing overhead, and operational complexity of legacy systems like SAP, Oracle NetSuite, and Dynamics 365.

### The Problem FusionOS Solves

1. **Siloed Data & Fragmented Systems** — accounting, inventory, and sales often live in separate tools stitched together with fragile batch scripts.
2. **High License & Implementation Cost** — legacy ERP vendors charge heavily for per-user licenses and mandatory implementation consultants.
3. **Dated User Interfaces** — many incumbent ERPs still run 2000s-era desktop UIs.
4. **Multi-Tenant Isolation Risk** — SaaS backends that get tenant-scoping wrong leak cross-customer data.

### Core Philosophy

- **Tenant isolation and RBAC by default** — every request passes through pipeline behaviors (`AuthorizationBehavior`, `TenantIsolationBehavior`) before it reaches a handler; permission checks fail closed rather than silently passing unmarked requests through.
- **Modular Monolith first** — clean domain separation without premature microservice overhead. Modules communicate via strongly-typed MediatR commands and Kafka integration events.
- **Honest about where it stands** — this README and the linked docs describe what's actually implemented today, not just what's designed. See [Security & Compliance](#-security--compliance) for a concrete example of that distinction.

---

## 🖼 Screenshots

Real captures from a running instance (Luxor, the platform's own demo/audit tenant) — not mockups.

<table>
  <tr>
    <td width="50%"><img src="assets/screenshots/dashboard.png" alt="Executive Operating Dashboard" /><p align="center"><b>Executive Operating Dashboard</b></p></td>
    <td width="50%"><img src="assets/screenshots/finance.png" alt="Finance — Chart of Accounts" /><p align="center"><b>Finance — Chart of Accounts</b></p></td>
  </tr>
  <tr>
    <td width="50%"><img src="assets/screenshots/inventory.png" alt="Inventory — Product Catalog" /><p align="center"><b>Inventory — Product Catalog</b></p></td>
    <td width="50%"><img src="assets/screenshots/manufacturing.png" alt="Manufacturing — BOM & Work Centers" /><p align="center"><b>Manufacturing — BOM & Work Centers</b></p></td>
  </tr>
  <tr>
    <td width="50%"><img src="assets/screenshots/warehouse.png" alt="Warehouse Operations" /><p align="center"><b>Warehouse Operations</b></p></td>
    <td width="50%"><img src="assets/screenshots/procurement.png" alt="Procurement — Suppliers & Purchase Orders" /><p align="center"><b>Procurement — Suppliers & Purchase Orders</b></p></td>
  </tr>
</table>

---

## 🏗 Architecture & Design Principles

FusionOS is structured as a **Modular Monolith** using Clean Architecture patterns within each domain module.

```
                                  ┌───────────────────────────────────────────┐
                                  │           React 18 SPA (Vite Web)         │
                                  │        + installable PWA (offline shell)  │
                                  └─────────────────────┬─────────────────────┘
                                                        │ HTTP / REST
                                                        ▼
                                  ┌───────────────────────────────────────────┐
                                  │             FusionOS.Api.Host             │
                                  │   (JWT Auth • Rate Limit • ProblemDetails)│
                                  └─────────────────────┬─────────────────────┘
                                                        │ MediatR Pipeline
               ┌────────────────────────────────────────┼────────────────────────────────────────┐
               ▼                                        ▼                                        ▼
   ┌───────────────────────┐                ┌───────────────────────┐                ┌───────────────────────┐
   │  Core / Auth / Tenant │                │  Finance / Accounting │                │ Manufacturing & MRP   │
   └───────────┬───────────┘                └───────────┬───────────┘                └───────────┬───────────┘
               │                                        │                                        │
               └────────────────────────────────────────┼────────────────────────────────────────┘
                                                        │ EF Core / IDbContext (one per module)
                                                        ▼
                                  ┌───────────────────────────────────────────┐
                                  │          PostgreSQL 16 Database           │
                                  │   (Multi-Tenant Schema • xmin Concurrency)│
                                  └──────────────┬────────────────────┬───────┘
                                                 │                    │
                                                 ▼                    ▼
                                     ┌───────────────────────┐  ┌───────────┐
                                     │ Redis (Cache & Locks) │  │ Kafka Bus │
                                     └───────────────────────┘  └───────────┘
```

### Key Architectural Highlights

1. **MediatR Pipeline Behaviors** (`backend/src/Shared/FusionOS.BuildingBlocks.Application/Behaviors/`):
   - `LoggingBehavior` — logs incoming requests and execution time.
   - `ValidationBehavior` — runs FluentValidation rules before domain processing.
   - `AuthorizationBehavior` — RBAC permission checks; fails closed on any request that doesn't declare itself either permission-gated or explicitly public.
   - `TenantIsolationBehavior` — validates a request's `CompanyId` against the caller's JWT claim.
   - `AuditBehavior` — records mutations to an audit trail.
   - `PlanEntitlementBehavior` — gates module access by the company's Billing subscription tier.
2. **PostgreSQL Optimistic Concurrency** — every entity uses the Postgres `xmin` system column (`UseXminAsConcurrencyToken()`) for concurrency control.
3. **Kafka Event Streaming** — cross-module integration events (e.g., a goods receipt confirming a purchase order) are published asynchronously for eventual consistency across domains.
4. **Real EF Core migrations** for every module (not `EnsureCreated`), so the schema is versioned and upgradeable in place.

---

## 📦 Enterprise Module Overview (16 Modules)

### 1. 🛡️ Core & Tenant Administration
Multi-tenant bootstrap, RBAC (roles/permissions/segregation-of-duties conflict rules), branches/departments, document management, global search, feature flags, audit logging.

### 2. 💰 Finance & General Ledger
Chart of accounts, journal entries, recurring vouchers, fiscal period close, financial statements (Balance Sheet, P&L, Cash Flow), GST/e-invoicing/e-way-bill/TDS-TCS compliance, FX revaluation, multi-entity consolidation with intercompany elimination.

### 3. 📦 Inventory & Stock Ledger
Product master with generic attributes, immutable stock ledger, batch & serial tracking, barcode/QR generation, unit-of-measure conversions.

### 4. 🏬 Warehouse Operations
Warehouse → Zone → Rack → Shelf → Bin hierarchy, goods receipt & putaway, pick-list assignment/packing, cycle counts.

### 5. ⚙️ Manufacturing & MRP
Multi-level, versioned BOMs; work center & machine capacity planning; multi-level MRP explosion; work order operations, rework, and WIP reporting.

### 6. 🛒 Procurement & Purchasing
Supplier directory & scorecards, RFQ/quote comparison, multi-tier PO approval workflow, three-way match (PO ↔ GRN ↔ AP), vendor returns.

### 7. 🤝 CRM & Sales Pipeline
Leads/opportunities with scoring and Kanban pipeline, campaigns, territory/owner assignment, duplicate detection, activity timeline.

### 8. 🏷️ Sales & Customer Portal
Quotations → sales orders → dispatch → invoice, discount rules, credit notes, returns/RMA, a self-service customer portal.

### 9. 👥 HRMS & Payroll
Employee directory, leave management with balances/carry-forward, onboarding/offboarding workflows, payroll → Finance GL posting.

### 10. 🔬 Quality Control
Inspection plans (incoming/in-process/final), non-conformance reports, CAPA, quality holds, Certificates of Analysis, SPC and calibration tracking.

### 11. 🔧 Maintenance & Asset Operations
Asset registry, preventive maintenance scheduling, meter-reading triggers, spare-parts usage, MTBF/MTTR downtime analytics.

### 12. 📊 Business Intelligence & Reporting
Live-query executive dashboards and KPI definitions computed directly against operational data (not manual snapshots).

### 13. 🤖 AI Platform & Copilot
Natural-language search, demand forecasting, invoice OCR ingestion, and an AI copilot grounded in RBAC-scoped retrieval.

### 14. 🔌 Integration Hub
Connector framework for Shopify/ONDC/payment gateways/shipping providers with signature-verified inbound webhooks; ETL migration tooling for Tally/SAP/legacy data.

### 15. 🏪 Plugin Marketplace
Third-party extension framework with installable/toggleable plugin listings.

### 16. 💳 Billing & Subscriptions
Plan catalog, per-company subscriptions with trial/active/past-due status, usage records, and plan-tier module entitlement enforcement.

---

## ⚙️ Tech Stack Matrix

| Layer | Technologies Used | Details & Highlights |
| :--- | :--- | :--- |
| **Backend Framework** | .NET 8 (C#) | Clean Architecture, CQRS, MediatR, FluentValidation |
| **Frontend Framework** | React 18, TypeScript | SPA, Vite 5, React Router v6, installable PWA |
| **Styling & UI** | TailwindCSS, Lucide Icons | Responsive design, dark mode |
| **Database** | PostgreSQL 16 | Multi-DbContext-per-module, EF Core 8, `xmin` concurrency, real versioned migrations |
| **Caching & Locking** | Redis 7 | Distributed cache, distributed locks |
| **Event Bus / Messaging**| Apache Kafka | Cross-module integration events |
| **Authentication** | JWT (HS256) + BCrypt + rotating refresh tokens | See [Security & Compliance](#-security--compliance) for what this does and doesn't cover today |
| **Observability** | OpenTelemetry, Prometheus, Grafana, Loki | Distributed tracing, metrics, log aggregation |
| **DevOps & Containers** | Docker, Docker Compose, Nginx | Multi-stage production builds, reverse proxy |
| **Distribution** | Inno Setup (Windows), self-contained package (macOS), PWA (any platform) | See [Download FusionOS](#%EF%B8%8F-download-fusionos) |
| **Testing** | xUnit, Vitest, React Testing Library, Playwright | Unit, integration, and E2E coverage |

---

## 📂 Directory Structure

```
FusionOS/
├── .github/                      # CI/CD workflows, Dependabot config, issue templates
├── assets/                       # SVG branding, hero banner, architecture diagrams
├── backend/                      # .NET 8 Modular Monolith Solution
│   ├── FusionOS.sln
│   ├── src/
│   │   ├── Host/                 # FusionOS.Api.Host — Web API entrypoint, serves the SPA in single-EXE builds
│   │   ├── Modules/               # 16 independent enterprise domain modules
│   │   │   ├── Ai/ Billing/ BusinessIntelligence/ Core/ Crm/ Finance/
│   │   │   ├── Hrms/ IntegrationHub/ Inventory/ Maintenance/ Manufacturing/
│   │   │   └── Marketplace/ Procurement/ Quality/ Sales/ Warehouse/
│   │   └── Shared/                # BuildingBlocks — MediatR pipeline behaviors, base entities
│   └── tests/                    # xUnit unit and integration test projects
├── docs/
│   ├── api/                      # OpenAPI spec & endpoint guides
│   ├── architecture/             # Diagrams and system flow specs
│   ├── blueprint/                # Master architecture/design blueprint documents
│   ├── compliance/iso27001/       # ISO 27001 gap analysis, SoA, ISMS policies, remediation roadmap
│   ├── database/                 # ERD and migration guides
│   └── deployment/               # Production/on-prem deployment runbooks
├── installer/                    # Windows Inno Setup script + launcher scripts; macOS setup/launch scripts
├── frontend/                     # React 18 + Vite + TypeScript SPA
│   ├── public/                   # manifest.json, service worker, downloadable installers
│   └── src/
├── observability/                # Prometheus, Grafana, OTel Collector configs
├── scripts/                      # Backup/restore, migration, and installer-build automation
├── docker-compose.yml
├── README.md
├── LICENSE
└── CONTRIBUTING.md
```

---

## 🚀 Installation Guide

### ⬇️ External Software & Tool Download Links

| External Software / Tool | Required Version | Official Download Link | Purpose |
| :--- | :--- | :--- | :--- |
| ⚡ **.NET 8.0 SDK** | `v8.0.x LTS` | [Download](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) | Backend compilation & API execution |
| 💚 **Node.js (LTS)** | `v20.x LTS` | [Download](https://nodejs.org/en/download/) | Frontend SPA runtime & npm |
| 🐳 **Docker Desktop** | `v24.0+` | [Download](https://www.docker.com/products/docker-desktop/) | Containerized Postgres/Redis/Kafka/OTel stack |
| 🐙 **Git** | `v2.40+` | [Download](https://git-scm.com/downloads) | Source control |
| 🐘 **PostgreSQL** | `v16.x` | [Download](https://www.postgresql.org/download/) | Standalone DB setup without Docker |
| 🔴 **Redis** | `v7.x` | [Download](https://redis.io/download/) | Distributed cache/locking |

### 🪟 Windows (from source)

```powershell
git clone https://github.com/worksaransh/FusionOS.git
cd FusionOS
docker compose up -d postgres redis kafka otel-collector

cd backend
dotnet restore FusionOS.sln
dotnet build FusionOS.sln
cd ..

cd frontend
npm install
npm run dev
```
Then open `http://localhost:5173` (web app), `http://localhost:5000` (API), `http://localhost:5000/swagger` (API docs).

### 🍎 macOS (from source)

```bash
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
brew install git node@20
brew install --cask docker
brew install dotnet-sdk   # or the official .NET 8 installer

git clone https://github.com/worksaransh/FusionOS.git
cd FusionOS
docker compose up -d postgres redis kafka otel-collector

cd frontend && npm install && npm run dev
```

### 🐧 Ubuntu / Linux

```bash
sudo apt update && sudo apt install -y git curl build-essential
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt install -y nodejs dotnet-sdk-8.0

git clone https://github.com/worksaransh/FusionOS.git
cd FusionOS
docker compose up --build -d
```

### 🐳 Docker Compose (all platforms, full stack)

```bash
docker compose up --build -d   # start everything
docker compose logs -f          # follow logs
docker compose down             # stop
```
Access: Web `http://localhost:3000` • API `http://localhost:5000` • Swagger `http://localhost:5000/swagger` • Grafana `http://localhost:3001` • Prometheus `http://localhost:9090`.

---

## ⬇️ Download FusionOS

FusionOS ships in four forms, in addition to running from source:

| Platform | What it is | Where to get it |
| :--- | :--- | :--- |
| 🪟 **Windows installer** | A single Inno Setup `.exe` that installs a self-contained FusionOS build (API + bundled SPA in one process) and walks you through Postgres connection setup on first run. | Built via [`scripts/build-windows-installer.ps1`](scripts/build-windows-installer.ps1); the Download button on the marketing landing page serves the built artifact. Full steps: [`docs/INSTALL_WINDOWS.md`](docs/INSTALL_WINDOWS.md). |
| 🍎 **macOS package** | A self-contained `osx-arm64`/`osx-x64` build with setup/launch/stop shell scripts. Unsigned/unnotarized — see the README inside the package for the Gatekeeper first-run step. | Built via [`scripts/build-mac-package.ps1`](scripts/build-mac-package.ps1). Full steps: [`docs/INSTALL_MACOS.md`](docs/INSTALL_MACOS.md). |
| 📱 **PWA (iOS / Android / desktop)** | The web app is an installable Progressive Web App — "Add to Home Screen" on iOS/Android, or install from the browser's address bar on desktop. Caches the app shell for fast loads; never caches API data offline (financial data is always fetched live). | Open the deployed web app and use your browser's install/Add-to-Home-Screen action. |
| 🐳 **Docker Compose** | The full production-shaped stack (API, SPA, Postgres, Redis, Kafka, observability) as one command. | See [Docker Compose](#-docker-compose-all-platforms-full-stack) above. |

Both the Windows installer and the on-prem Docker Compose profile are self-hosted — you bring your own PostgreSQL and your own domain/reverse-proxy. See [`docs/deployment/ONPREM_RUNBOOK.md`](docs/deployment/ONPREM_RUNBOOK.md) for the full on-prem operations guide, including the backup/restore drill and known limitations of the current single-machine deployment shape.

---

## 🔑 Environment Variables Reference

| Variable | Required | Default | Description |
| :--- | :---: | :--- | :--- |
| `ASPNETCORE_ENVIRONMENT` | Yes | `Development` | `Development` / `Staging` / `Production` |
| `ConnectionStrings__Postgres` | Yes | — | PostgreSQL connection string |
| `Jwt__SigningKey` | Yes | — | JWT signing secret — required outside Development, app fails fast if unset |
| `Jwt__Issuer` / `Jwt__Audience` | Yes | `FusionOS.Host` / `FusionOS.App` | JWT claims |
| `Deployment__AutoMigrate` | No | `false` | Opt-in flag allowing migration-on-boot outside Development (used by the Windows installer's single-EXE build) |
| `REDIS_HOST` | Yes | `localhost` | Redis hostname |
| `KAFKA_BOOTSTRAP_SERVERS` | Yes | `localhost:9092` | Kafka broker address |
| `VITE_API_BASE_URL` | Yes | `http://localhost:5000/api/v1` | Frontend API base URL |

None of these should be committed with real production values — see [Security & Compliance](#-security--compliance) for the current state of secrets handling and where it falls short of that goal.

---

## 📡 API Documentation

All protected endpoints require a Bearer token:
```http
Authorization: Bearer <your_jwt_access_token>
```

| Module | Method | Endpoint | Description |
| :--- | :---: | :--- | :--- |
| Auth | `POST` | `/api/v1/core/auth/login` | Login & JWT issuance (rate-limited: 5/min) |
| Auth | `POST` | `/api/v1/core/auth/refresh` | Rotate an opaque refresh token for a new access token |
| Company | `POST` | `/api/v1/core/companies` | Bootstrap a new tenant company |
| Inventory | `GET/POST` | `/api/v1/inventory/products` | Product catalog |
| Manufacturing | `POST` | `/api/v1/manufacturing/mrp/run` | Run MRP explosion |
| Finance | `GET` | `/api/v1/finance/accounts` | Chart of accounts |
| Finance | `GET` | `/api/v1/finance/reports/consolidated-trial-balance` | Multi-entity consolidated trial balance |
| Billing | `GET` | `/api/v1/billing/plans` | Plan catalog |

Full interactive documentation: `http://localhost:5000/swagger` once the API is running.

---

## 🗄 Database & Data Architecture

PostgreSQL 16 with one `DbContext` per module for clean schema separation, and real, versioned EF Core migrations for every module (applied via `dotnet ef database update`, not `EnsureCreated`).

```
                          ┌─────────────────────────────────────────┐
                          │         PostgreSQL Database             │
                          └────────────────────┬────────────────────┘
                                               │
     ┌──────────────────┬──────────────────────┼──────────────────────┬──────────────────┐
     ▼                  ▼                      ▼                      ▼                  ▼
┌──────────────┐ ┌──────────────┐     ┌──────────────────┐  ┌──────────────────┐ ┌──────────────┐
│ CoreDbContext│ │FinanceDbCtx  │     │InventoryDbContext│  │ManufactureDbCtx  │ │ SalesDbContext│
└──────────────┘ └──────────────┘     └──────────────────┘  └──────────────────┘ └──────────────┘
                          … one DbContext per remaining module (Billing, HRMS, Quality, etc.)
```

### Backup & Restore

```bash
./scripts/backup-postgres.sh                                    # pg_dump, custom format, local retention
./scripts/restore-postgres.sh backups/fusionos_backup_....dump   # interactive, confirms before restoring
```
**Current limitation, stated plainly:** these scripts work and have been exercised in a restore drill, but nothing in this repo schedules them automatically yet, and the backup output isn't encrypted. See [`docs/compliance/iso27001/REMEDIATION_ROADMAP.md`](docs/compliance/iso27001/REMEDIATION_ROADMAP.md) for the plan to close that.

---

## 🚀 Deployment Guide

### Production Nginx Reverse Proxy

```nginx
server {
    listen 80;
    server_name erp.yourdomain.com;

    location / {
        root /var/www/fusionos/frontend/dist;
        try_files $uri $uri/ /index.html;
    }

    location /api/ {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

For a full on-prem, no-Docker deployment (the same shape the Windows installer produces), see [`docs/deployment/ONPREM_RUNBOOK.md`](docs/deployment/ONPREM_RUNBOOK.md).

---

## 🔐 Security & Compliance

FusionOS enforces RBAC and tenant isolation centrally via MediatR pipeline behaviors, hashes passwords with BCrypt, rotates refresh tokens on use, rate-limits authentication endpoints specifically, and verifies inbound webhook signatures for its payment/commerce connectors.

It also has real, specific gaps — no field-level encryption for sensitive data yet, no secrets manager (config-based secrets today), no second authentication factor, and tenant isolation currently lives only in the application layer rather than also at the database level. We'd rather state that plainly than let a badge or a marketing paragraph imply otherwise.

A full, evidence-based security posture — control-by-control against ISO/IEC 27001:2022 Annex A, with file:line citations for every claim — lives in [`docs/compliance/iso27001/`](docs/compliance/iso27001/):

- [`GAP_ANALYSIS.md`](docs/compliance/iso27001/GAP_ANALYSIS.md) — what's actually implemented today, control by control.
- [`STATEMENT_OF_APPLICABILITY.md`](docs/compliance/iso27001/STATEMENT_OF_APPLICABILITY.md) — the formal SoA an auditor requests first.
- [`ISMS_POLICIES.md`](docs/compliance/iso27001/ISMS_POLICIES.md) — the core policy set (access control, cryptography, secure development, backup/DR, incident response, and more).
- [`REMEDIATION_ROADMAP.md`](docs/compliance/iso27001/REMEDIATION_ROADMAP.md) — every gap, prioritized and sequenced.

To report a security issue, see [`SECURITY.md`](SECURITY.md).

---

## 🔧 Troubleshooting & FAQ

**`dotnet build` fails with "No .NET SDKs were found"?**
Ensure .NET 8 SDK is installed and on PATH — verify with `dotnet --info`.

**Frontend shows 401 Unauthorized on API calls?**
Confirm `Jwt__SigningKey` matches between your `.env` and the backend, and is at least 32 characters.

**Database connection fails on `docker compose up`?**
Ensure the Postgres container's health check passes before the backend starts — check with `docker compose ps`.

**Windows installer shows an "unknown publisher" warning?**
Expected today — the installer isn't code-signed yet (tracked in the ISO 27001 remediation roadmap). Choose "More info → Run anyway" if you trust the source you downloaded it from.

---

## 🤝 Contributing & Community

- 📖 [Contributing Guidelines](CONTRIBUTING.md)
- 📜 [Code of Conduct](CODE_OF_CONDUCT.md)
- 🔒 [Security Policy](SECURITY.md)
- 📜 [Changelog](CHANGELOG.md)

---

## 📄 License

FusionOS is open-source software licensed under the **[MIT License](LICENSE)**.

<div align="center">
  <sub>Built with ❤️ by Saransh Gulati and the FusionOS open-source community.</sub>
</div>
