# 02_TECH_STACK.md — FusionOS Technology Decisions

Status: APPROVED BASELINE (v1.0) — binding on all modules unless superseded by a future revision of this document.

This document is the single source of truth for technology choices. Every other blueprint document, and every implementation prompt after it, must reference these decisions rather than re-deciding them. If a module has a genuine reason to deviate (e.g., AI/ML workloads needing Python), the deviation must be justified in `12_AI_PLATFORM.md` or a future ADR (Architecture Decision Record) — never chosen ad hoc mid-implementation.

## 1. Guiding Criteria

Technology was chosen against five criteria, in this priority order:
1. **Long-term maintainability at millions-of-lines scale** — strong typing, mature tooling, refactor safety.
2. **Enterprise track record** — proven in ERP-class systems (SAP, Dynamics, NetSuite all run on statically typed, strongly governed stacks).
3. **Team scalability** — a stack many engineers can be hired into and onboarded onto quickly.
4. **Modular monolith fit** — must support strict module boundaries, DI, and in-process eventing without forcing premature microservices.
5. **AI-platform readiness** — first-class support for background processing, event streaming, and integration with ML/LLM services.

## 2. Backend Platform

| Layer | Decision | Rationale |
|---|---|---|
| Language/Runtime | **.NET 8 (C#), LTS** | Strong static typing, best-in-class DI container, async/await as a first-class citizen, mature enterprise adoption (Dynamics 365 itself is .NET), excellent performance per core. |
| Web framework | **ASP.NET Core Web API** | Native OpenAPI support, middleware pipeline for cross-cutting concerns (auth, logging, rate limiting), minimal APIs or controllers per module preference. |
| Application pattern | **Clean Architecture + CQRS via MediatR** | Separates Domain / Application / Infrastructure / API layers; commands and queries are independently testable and support the event-driven module boundary described in `03_SYSTEM_ARCHITECTURE.md`. |
| ORM / Data access | **Entity Framework Core (Npgsql provider)** with **Repository + Unit of Work pattern** wrapping it | EF Core gives migrations and change tracking; the repository layer keeps domain code persistence-ignorant per DDD. |
| Validation | **FluentValidation** | Declarative, testable, composes with MediatR pipeline behaviors. |
| Background jobs | **Hangfire** (in-monolith) evolving to dedicated workers as load grows | Reliable recurring/scheduled jobs (reorder points, forecast runs, report generation) without a separate scheduler service on day one. |

**Alternatives considered and rejected:** Java/Spring Boot (equally valid, but .NET has a slightly faster inner-loop dev experience and stronger first-party Azure/enterprise tooling); Node.js/NestJS (weaker type guarantees at this scale, less mature transactional ORM story); Python/Django (excellent for AI/ML, poor fit for the core transactional ledger where type safety and performance matter most — this is why Python is scoped only to the AI module, see `12_AI_PLATFORM.md`).

## 3. Data Layer

| Component | Decision | Rationale |
|---|---|---|
| Primary database | **PostgreSQL 16+** | Per project rules. ACID, JSONB for semi-structured extensions, mature partitioning, strong indexing (GIN/GiST/BRIN), row-level security usable for defense-in-depth on multi-tenancy. |
| Caching | **Redis 7** | Session/token cache, distributed locks, hot-read caching (product catalog, pricing), pub/sub for lightweight in-cluster signaling. |
| Full-text & advanced search | **PostgreSQL FTS + pg_trgm** at MVP; **OpenSearch** once catalog/search volume justifies it (target: >250k SKUs or sub-second faceted search requirements) | Avoids operating a second search cluster before it's needed, but the schema is designed so migration is additive, not a rewrite. |
| Vector store (AI/RAG) | **pgvector extension on PostgreSQL** initially; **Qdrant** if vector volume/latency demands it | Keeps AI embeddings close to transactional data early; documented upgrade path in `12_AI_PLATFORM.md`. |
| Migrations | **EF Core Migrations**, one migration project per module schema | Keeps module data ownership explicit; no module may migrate another module's tables. |

## 4. Messaging & Eventing

| Component | Decision | Rationale |
|---|---|---|
| In-process events | **MediatR `INotification`** | Cheap, synchronous or fire-and-forget in-process domain events between modules inside the monolith. |
| Durable/cross-module events | **Transactional Outbox pattern** writing to a `domain_events` table, relayed by a background publisher into **Apache Kafka** (preferred) or **RabbitMQ** (acceptable lighter-weight start) | Guarantees at-least-once delivery, decouples modules truly (no direct calls between module services), and is the same mechanism that will let any module be extracted into its own service later without a rewrite. Full contract in `03_SYSTEM_ARCHITECTURE.md`. |
| Event schema/versioning | **CloudEvents envelope + JSON Schema per event type, versioned (`InventoryAdjusted.v1`)** | Prevents silent breaking changes as modules evolve independently. |

## 5. Frontend Platform

| Layer | Decision | Rationale |
|---|---|---|
| Framework | **React 18 + TypeScript** | Per project rules; TypeScript is non-negotiable, no `any` escape hatches without justification (see `09_CODING_STANDARDS.md`). |
| Build tooling | **Vite** | Fast dev server and builds at scale, first-class TS support. |
| State/data | **TanStack Query** (server state) + **Zustand** (client/UI state) | Avoids over-centralizing server data into a monolithic Redux store; server cache and UI state are different problems and are treated as such. |
| Styling/design system | **Tailwind CSS + shadcn/ui (Radix primitives)** | Fast to build enterprise-dense UIs, accessible by default, themeable for dark mode (see `06_UI_UX_DESIGN_SYSTEM.md`). |
| Forms | **React Hook Form + Zod** | Type-safe schema validation shared conceptually with backend FluentValidation rules. |
| Routing | **React Router v6** | Standard, supports code-splitting per module. |

## 6. API Layer

- **Protocol:** REST over HTTPS, OpenAPI 3.1 spec generated from code (Swashbuckle), never hand-maintained separately from source.
- **Versioning:** URL-based, `/api/v1/...` (see `08_API_STANDARDS.md`).
- **Gateway:** none required at MVP monolith scale; **YARP reverse proxy** reserved as the seam if/when specific modules are extracted into services.

## 7. Security & Identity

- **AuthN:** JWT access tokens (short-lived, ~15 min) + rotating refresh tokens (see `07_SECURITY.md`).
- **AuthZ:** RBAC + fine-grained permission claims, enforced both in API middleware and reflected into frontend route/UI guards.
- **Future SSO:** OpenID Connect / SAML 2.0 federation via an external IdP (e.g., Azure AD/Entra, Okta, Keycloak) — designed for, not built, at MVP.

## 8. Testing & Quality

| Layer | Tooling |
|---|---|
| Unit tests (backend) | xUnit + FluentAssertions + NSubstitute |
| Integration tests (backend) | Testcontainers (real PostgreSQL, Redis, Kafka in CI) |
| Frontend unit/component | Vitest + React Testing Library |
| E2E | Playwright |
| Load/performance | k6, targeting the SLAs in `09_CODING_STANDARDS.md` |
| Static analysis | Roslyn analyzers + StyleCop (backend), ESLint + strict `tsconfig` (frontend), SonarQube for both |

## 9. Infrastructure & Operations

| Concern | Decision |
|---|---|
| Containerization | Docker, one image per deployable module boundary (even inside the monolith, so extraction later is trivial) |
| Orchestration | Docker Compose for local/dev; **Kubernetes** for staging/production once concurrent-user targets in `09_CODING_STANDARDS.md` require horizontal scaling |
| CI/CD | GitHub Actions — build, test, scan, migrate, deploy pipelines per environment |
| Observability | OpenTelemetry (traces/metrics/logs) → Prometheus + Grafana (metrics), Loki or Seq (logs), Jaeger/Tempo (traces) |
| Secrets | Cloud KMS/Key Vault equivalent; never in source or plain config |

## 10. Explicit Non-Goals (for now)

- No microservices split at MVP — the modular monolith with event-based decoupling is deliberate and documented in `03_SYSTEM_ARCHITECTURE.md`; premature service extraction is treated as a design violation.
- No NoSQL primary store — PostgreSQL JSONB covers semi-structured needs without giving up transactional integrity.
- No GraphQL at MVP — REST is the standard; GraphQL may be introduced later purely as an optional aggregation layer for the frontend, never as a replacement for module APIs.

## 11. Deployment Model — Cloud and On-Premise from One Codebase

Per the Product Requirements Document, FusionOS must run both as a multi-tenant cloud service and as a single-tenant on-premise install from the **same codebase**, with no forked branches. This is achieved by:

- **Configuration-driven tenancy:** a single deployment config flag (`DeploymentMode: Cloud | OnPrem`) toggles multi-tenant routing (shared cluster, `CompanyId` isolation) vs. single-tenant defaults (one company, isolation enforced by infrastructure instead of row filters).
- **Containerized artifacts only:** the same Docker images ship to SaaS Kubernetes clusters and to a customer's on-prem Docker Compose/K8s environment — never a separate on-prem build.
- **Pluggable infrastructure adapters:** object storage (S3-compatible — works against AWS S3 or on-prem MinIO), secrets (cloud KMS or local encrypted vault), and identity (hosted IdP or local Keycloak) are all behind interfaces so on-prem customers are never forced onto cloud-only dependencies.
- **Licensing module (Core):** on-prem deployments are gated by the License Management capability in Core (see `05_MODULE_ROADMAP.md`), not by code differences.
- **Offline-tolerant background jobs:** on-prem installs may lack constant internet; sync/queue-based patterns (outbox, retry-with-backoff) are mandatory, not optional, so this is never bolted on later.

## 12. Integration & Marketplace Layer

The PRD requires integrations (Shopify, WooCommerce, Amazon, Flipkart, ONDC, Shiprocket, Delhivery, Razorpay, Stripe, WhatsApp, Email) and a Marketplace (plugins, themes, report packs, workflow packs, industry extensions, AI agents). Architecturally these are never wired directly into core modules:

- **Integration Connectors** are a first-class module category under Core (`IntegrationHub`), each connector implementing a shared `IExternalConnector` contract (auth, sync, webhook-in, event-out) so new connectors are additive, never modifications to core modules.
- **Webhooks (inbound & outbound)** are a Core Platform capability, versioned identically to REST endpoints (see `08_API_STANDARDS.md`).
- **Marketplace/Plugin architecture** builds on the same module-boundary and event contracts used internally — an installable plugin is architecturally indistinguishable from an internal module, just packaged and permissioned separately. Full detail in `03_SYSTEM_ARCHITECTURE.md` §Plugin Architecture and `12_AI_PLATFORM.md` for AI agents specifically.

## 13. Change Control

Any change to this document requires: (1) a written rationale, (2) impact analysis against already-built modules, (3) sign-off recorded in this file's changelog before any code is written against the new decision.

**Changelog**
- v1.0 — Initial baseline established.
