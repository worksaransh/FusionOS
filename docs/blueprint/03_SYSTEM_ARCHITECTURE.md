# 03_SYSTEM_ARCHITECTURE.md — FusionOS System Architecture

## 1. Architectural Style: Modular Monolith

FusionOS is built as a **Modular Monolith**: one deployable application (per `02_TECH_STACK.md`, containerized identically for cloud and on-premise), internally partitioned into strictly bounded modules. This is a deliberate choice, not a stepping stone we're embarrassed about:

- Microservices add operational cost (distributed transactions, network latency, service mesh, eventual-consistency debugging) that is not justified until FusionOS has proven module boundaries and real independent scaling needs.
- A modular monolith enforced with the same rigor as microservices (no shared database schemas across modules, no direct in-process calls bypassing contracts, async eventing between modules) gets nearly all the maintainability benefit with none of the distributed-systems tax.
- Because every module is already event-decoupled and independently deployable-in-principle (own schema, own Docker image, own test suite), extracting any single module into a standalone service later is a deployment change, not a rewrite.

## 2. Module Boundary Rules

Every module (Core, Inventory, Warehouse, Procurement, Sales, Manufacturing, Finance, CRM, HRMS, Quality, Maintenance, Business Intelligence, AI, Marketplace/IntegrationHub — full roster in `05_MODULE_ROADMAP.md`) is a vertical slice with its own:

- **Domain layer** — entities, aggregates, value objects, domain events, invariants. No dependency on any other module's domain layer.
- **Application layer** — commands, queries, handlers (CQRS via MediatR), orchestration of its own domain plus published contracts of other modules.
- **Infrastructure layer** — its own EF Core `DbContext`/schema, its own repository implementations, its own external service adapters.
- **API layer** — its own versioned controllers/endpoints under `/api/v1/{module}/...`.
- **Public contract** — the *only* thing other modules may depend on: published DTOs, published domain events, and any explicitly exposed application service interface registered for cross-module use.

**Hard rule:** Module A must never query Module B's database tables directly, and must never take a project/package reference to Module B's Domain or Infrastructure layers. Cross-module reads go through Module B's published API or a read-model projection Module B owns; cross-module reactions go through events.

## 3. Layering (per module)

```
┌─────────────────────────────┐
│ API (controllers, DTOs)     │  ← depends on Application
├─────────────────────────────┤
│ Application (CQRS handlers) │  ← depends on Domain
├─────────────────────────────┤
│ Domain (entities, rules)    │  ← depends on nothing
├─────────────────────────────┤
│ Infrastructure (EF Core,    │  ← implements Domain interfaces
│ repositories, adapters)     │
└─────────────────────────────┘
```

Dependencies point inward only (Clean Architecture). Domain has zero framework references — no EF Core attributes leaking into domain entities, no HTTP concerns in application handlers.

## 4. Event Architecture

Two distinct eventing mechanisms serve two distinct purposes, and they are never conflated:

### 4.1 In-Process Domain Events (synchronous, same transaction)

Used for reactions that must happen atomically with the triggering change, within the same module or tightly related aggregates. Implemented as MediatR `INotification` handlers dispatched after the aggregate's changes are persisted but before the unit of work commits, or immediately after commit for non-transactional side effects (e.g., cache invalidation).

### 4.2 Cross-Module Integration Events (asynchronous, eventually consistent)

Used for all cross-module communication. Implementation:

1. A module completes a business transaction (e.g., Sales confirms a Sales Order) and, **in the same database transaction**, writes a row to that module's `domain_events` outbox table.
2. A background **Outbox Relay** (Hangfire job, one per module) polls/streams unpublished rows and publishes them to **Kafka** (or RabbitMQ at smaller scale) under a topic namespaced by module and event type, e.g. `sales.sales-order-confirmed.v1`.
3. Interested modules (e.g., Inventory, Finance) run their own consumers, idempotent by design (dedupe on event id), and react by executing their own commands.
4. This guarantees **at-least-once delivery** without ever coupling Sales to knowing Inventory or Finance exist.

**Event contract standard:** CloudEvents envelope (`id`, `source`, `type`, `time`, `datacontenttype`) wrapping a versioned JSON payload matching a published JSON Schema per event type. Payload changes are additive (new optional fields) within a version; breaking changes require a new version (`.v2`) with both versions published during a deprecation window.

**Representative event catalog (illustrative, not exhaustive):**

| Event | Producer | Key Consumers |
|---|---|---|
| `PurchaseOrderApproved.v1` | Procurement | Inventory (expected receipt), Finance (AP accrual) |
| `GoodsReceived.v1` | Warehouse | Inventory (ledger update), Procurement (PO status), Finance (AP match) |
| `SalesOrderConfirmed.v1` | Sales | Inventory (reservation), Warehouse (pick task), Finance (AR) |
| `ProductionOrderCompleted.v1` | Manufacturing | Inventory (finished goods receipt), Quality (inspection trigger), Finance (WIP relief) |
| `InventoryAdjusted.v1` | Inventory | Finance (valuation), BI (analytics), AI (forecasting signal) |
| `InvoicePosted.v1` | Finance | CRM (customer statement), BI |
| `EmployeeOnboarded.v1` | HRMS | Core (user/account provisioning), Notifications |

## 5. Multi-Tenancy & Multi-Company Architecture

- **CompanyId** is the tenancy discriminator embedded in every row (see `04_DATABASE_GUIDELINES.md`); a single database serves many companies in Cloud mode, enforced by application-level query filters plus PostgreSQL Row-Level Security as defense-in-depth.
- **BranchId** provides intra-company segmentation (branches/locations) beneath the company level.
- **Inter-company transactions** (a documented required scenario) are modeled as explicit domain concepts — e.g., an inter-company transfer creates linked ledger entries in both companies' books — never as a permission bypass that lets one company's module code reach into another's data.
- On-premise deployments typically run single-company but use the identical schema and code path; there is no "single-tenant mode" fork.

## 6. Plugin & Marketplace Architecture

The Marketplace (plugins, themes, report packs, workflow packs, industry extensions, AI agents) is built on the same seams used internally:

- A **Plugin** registers against the same event bus (subscribing to integration events) and the same extension-point interfaces (e.g., `IPricingRule`, `IValuationStrategy`, `IReportProvider`) that core modules use to allow variation without core modification.
- Plugins run in a constrained execution context with explicit permission grants (data scopes, API scopes) — never with ambient trust equal to core modules.
- Industry Extensions (e.g., pharma batch/expiry compliance, textile lot/shade tracking) are shipped as plugins layering additional validation/fields onto core entities via extension tables and events, keeping core schemas industry-agnostic.

## 7. Integration Hub Architecture

External integrations (Shopify, WooCommerce, Amazon, Flipkart, ONDC, Shiprocket, Delhivery, Razorpay, Stripe, WhatsApp, Email) live in a dedicated `IntegrationHub` module, never wired directly into Sales/Inventory/Finance code:

- Each connector implements a shared contract: inbound webhook handling → translates external payloads into FusionOS commands/events; outbound sync → subscribes to FusionOS integration events and calls the external API.
- Credentials/config per connector are company-scoped, encrypted at rest (see `07_SECURITY.md`).
- Failure isolation: a failing external API (e.g., Amazon rate-limited) degrades only that connector, never the core transactional path — enforced via the outbox/queue pattern, not synchronous calls from core modules.

## 8. Deployment View

```
                     ┌───────────────────────────┐
                     │        API Gateway /       │  (YARP, optional,
                     │  Reverse Proxy (future)    │   reserved seam)
                     └─────────────┬─────────────┘
                                   │
                     ┌─────────────▼─────────────┐
                     │   FusionOS Monolith (K8s)  │
                     │  Core | Inventory | WMS |  │
                     │  Procurement | Sales |     │
                     │  Manufacturing | Finance | │
                     │  CRM | HRMS | Quality |    │
                     │  Maintenance | BI | AI |   │
                     │  IntegrationHub            │
                     └──┬───────┬───────┬────────┘
                        │       │       │
                 ┌──────▼─┐ ┌───▼───┐ ┌─▼───────────┐
                 │Postgres│ │ Redis │ │ Kafka/RabbitMQ│
                 └────────┘ └───────┘ └─────────────┘
```

Each module's DbContext maps to its own PostgreSQL schema within the same physical database (simplifies operations at MVP scale); schema-per-module keeps the door open to splitting databases later without touching application code beyond connection routing.

## 9. Mobile & Client Architecture

Mobile applications (Warehouse, Sales, Management, Approvals, Production, Maintenance) consume the same versioned `/api/v1/` REST contracts as the web frontend — there is no separate "mobile API." Offline-tolerant scenarios (warehouse scanning with intermittent connectivity) use local queuing with idempotent replay against the same command endpoints.

## 10. What This Architecture Explicitly Forbids

Direct cross-module database joins or foreign keys; shared mutable in-memory state between modules; synchronous chained calls across more than one module boundary in a single request (prefer choreography via events); business logic embedded in the API/controller layer; and any module reaching into another module's Infrastructure layer for "convenience."
