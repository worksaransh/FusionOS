# 01_PROJECT_RULES.md — FusionOS Development Principles

Status: BINDING on all modules, all contributors, all AI-assisted code generation.

## Mission Restatement

Build FusionOS, a world-class Enterprise Business Operating System competing with SAP, Oracle, Microsoft Dynamics, Odoo, ERPNext, and NetSuite. This is not a CRUD application. This is an enterprise-grade ERP platform, and every line of code must be held to that standard.

## Development Principles (non-negotiable)

- Enterprise-grade code only. Production-ready implementation only.
- No demo code. No placeholder code. No mock data unless explicitly requested.
- No technical debt introduced knowingly. If a shortcut is unavoidable, it is logged as a tracked item, not silently left in.
- No duplicate logic — shared logic belongs in a shared kernel or explicitly designated shared library, never copy-pasted across modules.
- Follow SOLID principles in every class and module boundary.
- Follow Clean Architecture: Domain → Application → Infrastructure → Presentation/API, dependencies only point inward.
- Follow Domain-Driven Design: rich domain models, aggregates with enforced invariants, ubiquitous language shared between docs, code, and business stakeholders.
- Follow the Repository pattern where it adds value (aggregate persistence) — not as a reflexive wrapper around every table.
- Use Dependency Injection throughout; no static service locators, no `new` of infrastructure dependencies inside domain/application code.
- Use asynchronous programming end-to-end for I/O-bound work (`async`/`await`, non-blocking database and network calls).
- Every module must be independently testable — no module's unit tests require another module's database or running process.
- Every module must be scalable — stateless application services, horizontally scalable by design (see `02_TECH_STACK.md`).
- Every module must support future plugins — extension points (events, hooks, well-defined interfaces) are part of the module contract, not an afterthought.

## Before Writing Any Code

For every module or feature, the required sequence is:

1. **Analyze** — restate the business requirement, identify affected modules, cross-check against `04_DATABASE_GUIDELINES.md` and `05_MODULE_ROADMAP.md`.
2. **Design** — propose the domain model, API surface, events raised/consumed, and data model changes; check against `03_SYSTEM_ARCHITECTURE.md`, `08_API_STANDARDS.md`.
3. **Review** — explicitly check the design against security (`07_SECURITY.md`), performance targets (`09_CODING_STANDARDS.md`), and naming conventions before any code is written.
4. **Implement** — only after 1–3 are explicit and, where the process requires it, approved.

Claude (or any engineer) must never immediately start coding. Architecture is always explained and checked before implementation, every time, without exception — including for changes that look small.

## What Must Never Be Skipped in Any Generated Code

Validation · Authorization · Logging · Audit trail · Documentation · Automated tests · Error handling · Performance consideration · Security review. A feature that is "done" without all nine is not done — it is a partial draft.

## Architectural Shape

FusionOS is built as a **Modular Monolith** (full detail in `03_SYSTEM_ARCHITECTURE.md`). Representative modules: Core, Inventory, Warehouse, Procurement, Sales, Manufacturing, Finance, CRM, HRMS, Quality, Maintenance, Business Intelligence, AI, Marketplace. Modules communicate through events, never through direct database access into another module's schema and never through tight in-process coupling that bypasses published contracts.

## Database Principles (full detail in `04_DATABASE_GUIDELINES.md`)

PostgreSQL. UUID primary keys. Never hard-delete — every table supports soft delete via `IsDeleted`. Every table carries the mandatory audit/multi-tenancy columns (`Id`, `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`, `CompanyId`, `BranchId`, `IsDeleted`, `RowVersion`). Audit logging is a platform capability, not something each module reimplements.

## API Principles (full detail in `08_API_STANDARDS.md`)

REST, OpenAPI-documented, versioned under `/api/v1/`. Pagination, filtering, sorting, and search are standard on every list endpoint. Bulk operations are supported where the domain justifies them. Input validation and rate limiting are mandatory on every endpoint.

## UI Principles (full detail in `06_UI_UX_DESIGN_SYSTEM.md`)

React + TypeScript. Responsive. Dark mode. Keyboard shortcuts for power users. Fast loading. Enterprise UX — dense, data-forward, and efficient, not a consumer-app aesthetic stretched over enterprise data.

## Security Principles (full detail in `07_SECURITY.md`)

RBAC, JWT with refresh tokens, audit logs, permission-based UI (not just permission-checked APIs), encryption at rest and in transit, secure password storage, and a designed-in path to SSO.

## Performance Principles (full detail in `09_CODING_STANDARDS.md`)

Target scale: 500+ concurrent users, 1,000,000 SKUs, 100 warehouses, 100 companies, 10,000,000 inventory transactions. Response time targets: API < 300ms, dashboard < 2s, search < 1s. These targets are inputs to design decisions (indexing, caching, pagination), not aspirational numbers checked only at the end.

## Quality Bar

Every module ships with: database schema and migrations, API, frontend, unit tests, integration tests, input validation, documentation, error handling, structured logging, a performance review, and a security review. A module without all of these is incomplete, regardless of whether the "happy path" works.

## AI Rules (full detail in `12_AI_PLATFORM.md`)

AI is a platform feature woven through the product, not a chatbot bolted on top. AI must support forecasting, procurement recommendations, inventory optimization, production planning, OCR/document processing, natural-language search, automated report generation, predictive analytics, root-cause analysis, and workflow automation.

## Code Generation Rules

Any code generated — by a human or by Claude — must never skip: validation, authorization, logging, audit, documentation, tests, error handling, performance consideration, or security. This restates the section above deliberately: it is the rule most likely to be silently skipped under time pressure, and it is the rule most enforced in review.
