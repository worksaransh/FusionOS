# 04_DATABASE_GUIDELINES.md — FusionOS Database Standards

## 1. Engine

PostgreSQL 16+, per `02_TECH_STACK.md`. One physical database at MVP scale, one schema per module (`inventory.*`, `finance.*`, `manufacturing.*`, etc.). No cross-schema foreign keys between module schemas — cross-module references are stored as opaque UUIDs resolved via the owning module's API, never enforced as a DB-level FK across module boundaries (this preserves the module independence described in `03_SYSTEM_ARCHITECTURE.md`).

## 2. Primary Keys

Every table uses a **UUID (v7 preferred for time-ordered index locality)** primary key, generated application-side or via `gen_random_uuid()`/`uuid_generate_v7()`. Never auto-increment integers as primary keys — this is required for multi-company merge scenarios, offline-generated records (mobile, on-prem sync), and avoiding key collisions across tenant data sets.

## 3. Mandatory Columns (every table, no exceptions)

| Column | Type | Purpose |
|---|---|---|
| `Id` | `uuid` | Primary key |
| `CreatedAt` | `timestamptz` | Set once, server-side, UTC |
| `UpdatedAt` | `timestamptz` | Updated on every write |
| `CreatedBy` | `uuid` (FK → Core.Users) | Actor who created the row |
| `UpdatedBy` | `uuid` (FK → Core.Users) | Actor who last modified the row |
| `CompanyId` | `uuid` (FK → Core.Companies) | Tenant/company discriminator — mandatory even in on-prem single-company mode |
| `BranchId` | `uuid` (FK → Core.Branches), nullable only where a concept is genuinely company-wide | Location/branch discriminator |
| `IsDeleted` | `boolean`, default `false` | Soft-delete flag |
| `RowVersion` | `bytea`/`xmin`-based or explicit `uuid`/`bigint` version token | Optimistic concurrency control |

These columns are enforced via a shared EF Core base entity/interface (`IAuditable`, `ITenantScoped`, `ISoftDeletable`) and a global query filter — no module may opt out, and no hand-written query may bypass the `IsDeleted`/`CompanyId` filters except explicitly audited admin/reporting paths.

## 4. Soft Delete — Mandatory

Hard deletes are forbidden across the platform. `DELETE` statements are not issued by application code against business tables. Deletion is modeled as `IsDeleted = true` plus `DeletedAt`/`DeletedBy` (extension of the audit columns for delete-specific actors). Global EF Core query filters exclude soft-deleted rows by default; explicitly scoped queries (e.g., "show deleted items" admin views, audit reports) opt in deliberately.

Data retention/purge (for regulatory "right to erasure" scenarios) is a separate, explicitly authorized process — an archival/anonymization job, not an ad hoc `DELETE`.

## 5. Audit Logging

Audit logging is a **Core Platform capability**, not something each module reimplements:

- Every create/update/soft-delete on an auditable entity emits a structured audit record (`AuditLogs` table in Core schema): actor, timestamp, entity type, entity id, company/branch, before/after diff (JSONB), and the correlating request/trace id.
- Captured automatically via an EF Core `SaveChanges` interceptor plus MediatR pipeline behavior — modules do not hand-write audit calls for standard CRUD; they only add domain-specific audit events for significant business actions (e.g., "PO approved by X, overriding budget hold").
- Audit logs are themselves immutable (insert-only, no update/delete) and retained per compliance requirements per deployment (configurable retention, longer defaults for Finance-adjacent modules).

## 6. Multi-Company / Multi-Branch / Multi-Warehouse Modeling

Required scenarios (from the PRD) that the schema must support natively, not as later retrofits:

- Unlimited companies, unlimited branches per company, unlimited warehouses per branch or company.
- Inter-company transactions: modeled as paired, linked transactions (e.g., inter-company stock transfer creates a dispatch in Company A and a receipt in Company B, linked by a shared `TransferGroupId`), each still fully owned by its own `CompanyId`.
- One raw material used across multiple finished goods, one finished good with multiple BOM versions, alternative raw materials, multiple suppliers/customers, multiple price lists, bundles/kits — all modeled as first-class relational structures in Manufacturing/Inventory schemas (detailed per-module in `05_MODULE_ROADMAP.md`), not as JSONB free-for-all blobs.

## 7. Indexing & Performance

- Every `CompanyId` (and `CompanyId, BranchId` composite where relevant) is indexed — it is the most common filter predicate in the system given multi-tenancy.
- Every foreign key column is indexed.
- High-cardinality search columns (SKU, batch/serial number, barcode) use B-tree indexes; free-text fields use `pg_trgm`/GIN as described in `02_TECH_STACK.md`.
- Large transactional tables (Inventory Ledger, targeting 10M+ rows) are **partitioned** — by `CompanyId` range or by time (monthly/quarterly) depending on access pattern — decided per-table in that module's design doc, not left unpartitioned "until it's a problem."
- `EXPLAIN ANALYZE` review is a required step (see `01_PROJECT_RULES.md` — Review step) before any migration touching a table expected to exceed 1M rows ships.

## 8. Concurrency Control

Optimistic concurrency via `RowVersion` on every table is mandatory for any entity that can be edited concurrently (which is effectively all business entities). API layer returns `409 Conflict` with a machine-readable conflict payload when a `RowVersion` mismatch occurs (see `08_API_STANDARDS.md`).

## 9. Migrations

- One EF Core migration project per module; a module's migration may never alter another module's schema.
- Migrations are additive-first: new nullable columns, new tables, backfill jobs, then (in a later, separate migration) constraint tightening/column removal — no destructive migration ships in the same release as the feature that needs it, to keep rollback safe.
- All migrations run through CI against a Testcontainers PostgreSQL instance before merge; migration application is part of the deploy pipeline, never a manual `dotnet ef database update` against production.

## 10. Data Types & Conventions

- Monetary values: `numeric(19,4)` minimum, never `float`/`double` — required for Finance-grade correctness (GL, invoicing, valuation).
- Quantities: `numeric` with unit-of-measure stored alongside, never assume a fixed decimal precision across all UOMs.
- Timestamps: always `timestamptz`, stored UTC, converted at presentation layer per company/branch timezone setting.
- Enums: stored as `text`/lookup tables with a stable code column, not native PostgreSQL `enum` types (which are painful to alter at scale) or raw integers (which are unreadable in ad hoc queries).
- JSONB is permitted for genuinely schema-flexible extension data (e.g., industry-extension custom fields) — never as a substitute for proper relational modeling of core business data.

## 11. Backup, HA & Disaster Recovery

- Continuous WAL archiving plus scheduled base backups; point-in-time recovery required for both cloud and on-prem reference deployments.
- Streaming replication to at least one standby for cloud production; documented, tested restore procedure for on-prem customers as part of the on-prem deployment guide.
- RPO/RTO targets are defined per deployment tier in `09_CODING_STANDARDS.md` (performance/availability section) and validated with periodic restore drills, not assumed from backup existing.

## 12. Valuation & Ledger Integrity (Inventory/Finance-specific baseline)

Inventory valuation methods (FIFO, Weighted Average Cost) are computed from an **append-only Inventory Ledger** — valuation is derived, never stored as the sole source of truth and then manually adjusted. Any correction is a new ledger entry (reversal + correction), preserving a full audit trail consistent with `IsDeleted`/audit rules above; the ledger itself is never edited or hard-deleted.
