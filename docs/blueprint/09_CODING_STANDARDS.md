# 09_CODING_STANDARDS.md — Coding Standards, Naming Conventions & Performance Targets

## 1. General Coding Standards

- SOLID, Clean Architecture, DDD, Repository pattern, Dependency Injection, async-first — as mandated in `01_PROJECT_RULES.md`; this document is the concrete, checkable expression of those principles.
- No commented-out code committed. No `TODO` without a linked tracked work item. No magic numbers/strings — named constants or configuration.
- Nullability: nullable reference types enabled and enforced (C# `#nullable enable` project-wide); `strict` mode enabled in every `tsconfig.json`. Escape hatches (`!`, `any`, `// @ts-ignore`) require an inline justification comment and are flagged in review.
- Cyclomatic complexity and method-length lint rules enforced (Roslyn analyzers / ESLint) — large methods are a signal to extract, not a style nitpick to override.

## 2. Naming Conventions

| Context | Convention | Example |
|---|---|---|
| C# classes, methods, properties | `PascalCase` | `PurchaseOrderService`, `ApprovePurchaseOrder()` |
| C# private fields | `_camelCase` | `_unitOfWork` |
| C# interfaces | `I` prefix + `PascalCase` | `IInventoryRepository` |
| C# async methods | `PascalCase` + `Async` suffix | `GetProductByIdAsync()` |
| TypeScript variables, functions | `camelCase` | `fetchPurchaseOrders()` |
| TypeScript types/interfaces/components | `PascalCase` | `PurchaseOrderCard`, `type PurchaseOrder` |
| TypeScript constants (true constants) | `UPPER_SNAKE_CASE` | `MAX_PAGE_SIZE` |
| Database tables | `snake_case`, schema-prefixed, plural | `inventory.stock_adjustments` |
| Database columns | `snake_case` | `created_at`, `company_id` |
| JSON API fields | `camelCase` | `companyId`, `createdAt` |
| REST resource URLs | `kebab-case`, plural | `/api/v1/procurement/purchase-orders` |
| Domain events | `PascalCase` + `PastTense` + version | `PurchaseOrderApproved.v1` |
| Feature branches | `type/module-short-description` | `feat/inventory-cycle-count` |
| Commit messages | Conventional Commits | `feat(inventory): add cycle count reconciliation` |

Naming must use the same ubiquitous language across code, database, API, and documentation — if Finance calls it a "Cost Center" in the domain model, it is `cost_center` in the database, `costCenter` in the API, and "Cost Center" in the UI, never a different term at any layer.

## 3. Documentation Requirements

Every public application-service method and every API endpoint carries XML doc comments (backend) / TSDoc (frontend) sufficient to generate meaningful OpenAPI descriptions and IDE tooltips. Every module ships a `README.md` covering: purpose, domain concepts, published events (produced/consumed), and how to run its tests in isolation.

## 4. Testing Requirements

| Layer | Minimum bar |
|---|---|
| Domain layer | Unit tests for every invariant/business rule; aim for effectively complete coverage of aggregate behavior, not just a percentage target. |
| Application layer | Unit tests per command/query handler, including authorization and validation failure paths, not only the happy path. |
| Infrastructure/integration | Testcontainers-backed tests against real PostgreSQL/Redis/Kafka for repository and outbox behavior. |
| API | Contract tests validating the OpenAPI spec matches actual behavior. |
| Frontend | Component tests (Vitest/RTL) for interactive components; Playwright E2E for critical business flows per module (e.g., "create and approve a purchase order end to end"). |
| Cross-module | Consumer-driven contract tests for integration events, so a producer changing an event schema is caught before it breaks a consumer in CI, not in production. |

A pull request that reduces test coverage on touched code, or adds a handler with no corresponding test, does not merge — this is a gate, not a suggestion, per the Quality Bar in `01_PROJECT_RULES.md`.

## 5. Error Handling

- Domain layer throws domain-specific exceptions (`InsufficientStockException`, not generic `Exception`); these are mapped centrally to RFC 7807 problem responses at the API boundary (`08_API_STANDARDS.md` §6) — no module hand-rolls its own HTTP status mapping.
- Infrastructure failures (DB timeout, external API failure) are caught at the infrastructure boundary and translated into domain-meaningful outcomes (e.g., retry, queue for later, or a specific "external system unavailable" error) — they never leak raw stack traces or provider-specific exception types up to the API response.
- All errors are logged with correlation/trace id, actor, and company context sufficient to reproduce, without logging sensitive data (passwords, full payment credentials, tokens).

## 6. Logging & Observability

Structured logging (Serilog, JSON output) with a consistent set of enriched fields (trace id, company id, user id, module) on every log line, per `02_TECH_STACK.md` §9 (OpenTelemetry). Log levels used meaningfully: `Debug` for developer diagnostics, `Information` for business-significant events, `Warning` for recoverable anomalies, `Error` for failures requiring attention — not everything logged at `Information` or `Error` indiscriminately.

## 7. Performance Targets (binding)

**Scale targets:**

| Dimension | Target |
|---|---|
| Concurrent users | 500+ |
| Companies | 100 |
| Warehouses | 100 |
| Products/SKUs | 1,000,000 |
| Inventory transactions | 10,000,000 |

**Response time targets:**

| Operation class | Target |
|---|---|
| Standard API request | < 300ms (p95) |
| Dashboard load | < 2s |
| Search (any module) | < 1s |

**Availability targets:**

| Deployment tier | Target |
|---|---|
| Cloud production | 99.9% uptime SLA, documented RTO/RPO per `04_DATABASE_GUIDELINES.md` §11 |
| On-premise reference | Customer-operated, but FusionOS ships HA/DR runbooks meeting equivalent targets |

These targets inform design decisions at design time (indexing strategy, caching layer, pagination, partitioning — `04_DATABASE_GUIDELINES.md` §7) rather than being retrofitted after a module is slow. Every module's design review (`01_PROJECT_RULES.md` — Review step) explicitly states how it meets or scales toward these numbers.

## 8. Performance Review Gate

Before a module ships, its heaviest-traffic endpoints are load-tested (k6, per `02_TECH_STACK.md` §8) against realistic data volumes derived from the scale targets above — not against a handful of seed rows. Query plans for any query touching a table expected to exceed 1M rows are reviewed (`EXPLAIN ANALYZE`) as part of this gate.

## 9. Code Review Standard

Every change is reviewed against: correctness, adherence to this document's naming/structure conventions, test coverage, security implications (`07_SECURITY.md`), and performance implications (§7–8 above) before merge. "Looks fine" without checking these dimensions explicitly is not an adequate review.
