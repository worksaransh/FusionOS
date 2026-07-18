# FusionOS — Complete Implementation Plan (Phase 0 → Phase 18)

This turns `docs/DEVELOPMENT_ROADMAP.md` into an executable, phase-wise plan to take FusionOS from its current ~22% average completion to 100%. Each phase below is a self-contained prompt — copy the whole fenced block for one phase into a fresh session (or paste it into a continuation of this one) and it has everything needed to execute that phase without any other context.

**Run phases in order.** Each phase lists its prerequisites; do not start a phase before its prerequisites are done, per the dependency chain in `docs/DEVELOPMENT_ROADMAP.md` Step 5.

**Three phases are blocked on a decision only you can make** — inventory costing method (Phase 9), tax jurisdiction (Phase 8), notification delivery provider (Phase 7's delivery sub-step). Everything else can start as soon as its prerequisite phases are done.

**Phases 13–18 (the old "Phase F" set — Manufacturing, CRM, HRMS, Quality, Maintenance, Integration Hub, Marketplace, Mobile, Analytics, AI, SAP Migration) are included here so the complete path to 100% has a real plan behind it, exactly as you asked. They still should not be started without you saying go on that specific phase — nothing changes about that standing decision just because a plan now exists for them.**

---

## Conventions every phase must follow

These apply to every phase below and are not repeated in each block except by reference:

1. **Clean Architecture per module:** `backend/src/Modules/<Module>/FusionOS.Modules.<Module>.{Domain,Application,Infrastructure,Api}`. CQRS via MediatR. New permission-gated actions get a code in `PermissionCatalog.cs` following `<module>.<entity>.<verb>` and are enforced by the existing `AuthorizationBehavior` + `TenantIsolationBehavior` pipeline — do not hand-roll authorization checks in a handler.
2. **Soft-deactivate only.** Never a hard DELETE. Expose `POST {id}/deactivate`. `apiClient` (frontend) has no `delete` verb by design — do not add one.
3. **Immutable business keys.** Update commands exclude the entity's natural key (Sku, Code, BaseCurrency, etc.) — check the entity's Create command to see which field that is before writing Update.
4. **Error handling.** `ProblemDetailsExceptionHandler` already maps `ValidationException`→400, `ForbiddenException`→403, `InvalidOperationException`→409, `KeyNotFoundException`→404. Throw the right exception type; do not add new switch arms unless a genuinely new exception type is introduced.
5. **Frontend pattern.** react-hook-form + zod schema, `apiClient.get/post/put`, TanStack Query with `invalidateQueries` on success, list pages built on the shared `CrudListPage`/`DataTable`/`EntityCombobox` components — do not introduce a new list/table pattern.
6. **File-integrity discipline.** After every file write, verify it: no null bytes, balanced braces/parens, proper ending. If a write looks truncated, discard and rewrite the whole file via a heredoc, then re-verify. This environment has a history of silently truncating large writes.
7. **Verification.** Frontend: `tsc -b --force` must show 0 errors before a phase is considered done. Backend: this sandbox cannot compile .NET (no NuGet network access) — backend correctness is verified by careful code review and by matching the exact pattern of an already-working sibling entity, not by a compiler, until Phase 0 gives you a real build.
8. **Tracker.** Update `docs/PROJECT_TRACKER.md` — move the phase from "not started" to "completed" with a one-line note — after each phase finishes.

---

## Phase 0 — Environment — own-machine build & first migrations

**Prerequisites:** None — this is the first phase.
**Blocked on:** Needs your machine. This session's sandbox has no Docker daemon and no working NuGet network access, so this phase cannot be executed here.

```
Context: FusionOS is a .NET 8 modular monolith (backend/) + React 19/TypeScript frontend
(frontend/). Zero EF Core migrations have ever been generated for any of its 15 module folders,
and the backend has never been compiled. This phase proves the codebase actually runs.

On your own machine, in the repo root:

1. `cd backend && dotnet restore && dotnet build` — fix any real compiler errors you hit (there
   has never been a compiler check on this code before; expect some).
2. For each module with a DbContext (Core, Inventory, Warehouse, Procurement, Sales, Finance, plus
   the 9 scaffold modules), generate the first EF Core migration:
   `dotnet ef migrations add InitialCreate --project src/Modules/<Module>/FusionOS.Modules.<Module>.Infrastructure --startup-project src/Host/FusionOS.Api.Host`
3. `docker compose up -d` (Postgres, Kafka, and whatever else is declared in the repo's compose
   file) then apply migrations: `dotnet ef database update` per module (or via the Host project if
   migrations are centralized — check how `Program.cs` registers each module's DbContext first).
4. Run the API host (`dotnet run --project src/Host/FusionOS.Api.Host`) and hit one real endpoint
   per real module (Core health, Inventory products list, Warehouse warehouses list, Procurement
   suppliers list, Sales customers list, Finance accounts list) to confirm each returns 200, not a
   500 from a missing table or DI wiring bug.
5. Run the frontend (`cd frontend && npm install && npm run dev`) and confirm the app loads and can
   reach the API (check the network tab, not just that the page renders).

Report back: what broke, what you fixed, and paste any compiler/runtime error you couldn't resolve
so it can be diagnosed. This phase's output is the first ground-truth confirmation that anything in
`docs/DEVELOPMENT_ROADMAP.md` beyond "source code exists" is real.
```

---

## Phase 3 — Test coverage for ~60 untested handlers

**Prerequisites:** Phase 0 (need a real, running test database — in-memory/mocked tests can be written without it, but should be run against it before being trusted).

```
Context: the 6 real modules (Core, Inventory, Warehouse, Procurement, Sales, Finance) have CQRS
command/query handlers with essentially no test coverage. Add unit tests (handler logic, mocked
repositories) and integration tests (real DbContext, e.g. via Testcontainers or an EF InMemory
provider if already used elsewhere in the repo — check `backend/tests/` for the existing pattern
before introducing a new one).

Do this in 3 passes so it can run as parallel work if you choose:
Pass A — Core/Auth/RBAC/Companies (~15 handlers): Register, Login, RefreshToken, CreateRole,
  SetRolePermissions, AssignUserToRole, CreateCompany, UpdateCompany, DeactivateCompany,
  GetCompanyById, and the read-side query handlers gated in Phase H1.
Pass B — Inventory/Warehouse (~15 handlers): CreateProduct, UpdateProduct, DeactivateProduct,
  AdjustStock, GetStockOnHand, CreateWarehouse/Zone + their Update/Deactivate/GetById counterparts,
  CreateGoodsReceipt.
Pass C — Procurement/Sales/Finance (~30 handlers): Supplier CRUD, CreatePurchaseOrder + Approve,
  Customer CRUD, CreateSalesOrder, CreateInvoice (including the quantity-validation added in Phase
  M1), CreateDispatch, Account CRUD, CreateJournalEntry + Post.

For every handler test: cover the happy path, the permission-denied path (wrong role), the
tenant-isolation path (wrong companyId gets rejected), and at least one business-rule violation
(e.g. invoicing more than ordered, deactivating an already-inactive entity, self-approving a PO).

Target: 70%+ line/branch coverage on the Application layer of these 6 modules. Report the final
coverage number and list any handler you could not reasonably test (and why).
```

---

## Phase 4 — Orphaned events + AR payments

**Prerequisites:** Phase 0, Phase 3 (helps to have tests as a safety net, not strictly required).

```
Context: some integration events are published via the outbox but never consumed anywhere (a
"why does nothing react to this?" bug), and the Accounts Receivable ledger only ever increases
(InvoiceIssued adds a charge, but there is no way to record a payment against it).

1. Grep every `IntegrationEvent` (or equivalent marker) publish call in the codebase, and grep for
   every place `IIntegrationEventHandler`/consumer classes subscribe. List any event published with
   zero consumers. For each orphaned event, either (a) wire a real consumer if there's an obvious
   missing side effect (e.g. a Notification should have been created), or (b) if there's genuinely
   no missing side effect yet, leave it and note it explicitly rather than inventing a consumer
   that does nothing meaningful.
2. Add AR payment recording to Finance: a `RecordPaymentCommand` (CompanyId, InvoiceId, Amount,
   PaymentDate, Reference) that reduces the AR balance for that invoice, following the exact same
   Clean Architecture + CQRS + PermissionCatalog pattern as `CreateInvoice`. Add
   `finance.arpayment.create` to `PermissionCatalog.cs`. Add a frontend panel (same pattern as
   `InvoicesPanel`/`DispatchesPanel`) to record a payment against an invoice and show the
   updated outstanding balance.
3. Verify: `tsc -b --force` clean; grep confirms the new command/controller/permission exist; the
   AR balance query now reflects payments, not just charges.
```

---

## Phase 5 — Settings module + Search completion

**Prerequisites:** Phase 0.

```
Context: Settings is 0% — no entity, no CQRS, no UI. Search covers only 5 of 19 endpoints
(Product/Supplier/Customer/Warehouse/Account); Roles/Users/Audit Log/Companies/Permissions still
rely on client-side filtering only.

Part A — Settings: create `backend/src/Modules/Core/.../Settings/` following the exact same
Domain→Application→Infrastructure→Api layering as an existing Core entity. Minimum viable scope:
a per-company `CompanySettings` aggregate (default currency, default page size, display name/logo
URL). CRUD + Get. Add `core.settings.read`/`core.settings.update` to `PermissionCatalog.cs`. Add a
frontend Settings page under `frontend/src/modules/core/pages/`.

Part B — Search: for each of the 14 remaining endpoints (Roles, Users, Audit Log, Companies,
Permissions, and any others found missing by grepping for `EntityCombobox`/`entityOptions` usages
that lack a matching server-side search param), add the same `?search=` query-string handling
already implemented for Products/Suppliers/etc. — check
`backend/src/Modules/Inventory/.../ListProductsQueryHandler.cs` for the exact pattern to copy.

Verify: `tsc -b --force` clean; every one of the 19 originally-audited endpoints now accepts a
`search` parameter server-side.
```

---

## Phase 6 — Reports + Dashboard

**Prerequisites:** Phase 0, Phase 4 (Reports/Dashboard are far more useful once AR payments exist).

```
Context: Reports and Dashboard are both 0% — no CSV export, no canned reports, no KPI widgets,
despite there being plenty of real transactional data to report on.

1. Generic CSV export: add a `?format=csv` option (or a dedicated `/export` route) to the existing
   list endpoints for Products, Suppliers, Customers, Purchase Orders, Sales Orders, Invoices,
   Journal Entries — reuse the existing paged-query handlers, just change the serialization.
2. Three canned reports (new lightweight read-only endpoints, no new module needed — put them
   under whichever module owns the data): AR aging (group open invoices by days-overdue bucket),
   stock valuation (sum on-hand quantity × last known cost per product), PO status summary (count
   of POs by status: draft/approved/received).
3. Dashboard: a new frontend landing page with KPI widgets reading from endpoints that already
   exist or were just added — open Sales Orders count, pending PO approvals count, low-stock alert
   (products below a hardcoded or configurable threshold), AR aging summary.

Verify: `tsc -b --force` clean; each report returns real numbers when hit against a running
(Phase 0) database, not hardcoded placeholders.
```

---

## Phase 7 — Workflow engine + Notifications

**Prerequisites:** Phase 0.
**Blocked on:** Actual email/SMS/push delivery is blocked on you picking a provider (SendGrid/Twilio/SES/other) — the in-app notification UI and data model do not need this decision and can proceed now.

```
Context: approvals today are special-cased inside Procurement's PurchaseOrder.Approve() method —
there is no reusable engine. Notifications has a persisted entity but no create/list commands and
no UI; delivery (email/SMS/push) does not exist at all.

Part A — Workflow: design a generic `ApprovalRequest` aggregate (EntityType, EntityId, RequestedBy,
Status, Approvals[] with Approver/Decision/Timestamp) in a new or existing cross-cutting location
(discuss with yourself whether this belongs in Core or its own thin module — recommend Core, since
RBAC/Users already live there and every module will depend on it). Refactor
`PurchaseOrder.Approve()` to go through this generic engine instead of its own bespoke check,
without changing the self-approval-blocked business rule that already exists. Add
`core.approval.request`/`core.approval.decide` permissions.

Part B — Notifications: add `CreateNotificationCommand` and `ListUnreadNotificationsQuery` following
the standard CQRS pattern. Add a bell-icon dropdown in the frontend header showing unread count and
a list, with a mark-as-read action. Wire the new Workflow engine's approval-requested/approved/
rejected events to create in-app notifications for the relevant users.

Part C (blocked until you pick a provider): once you tell me which of SendGrid/Twilio/SES/other to
use, add an `INotificationDeliveryService` abstraction with that provider as the implementation,
and hook it to fire alongside the in-app notification for high-priority events (approval requests).

Verify: `tsc -b --force` clean; PO approval still enforces the self-approval rule after the
refactor (re-run or write the test from Phase 3 for that rule); bell icon shows real unread counts.
```

---

## Phase 8 — Finance depth

**Prerequisites:** Phase 0.
**Blocked on:** The tax engine sub-step is blocked until you tell me which tax jurisdiction(s) to support (single-country GST/VAT, or multi-country) — everything else in this phase does not depend on that decision.

```
Context: Finance currently has Chart of Accounts, Journal Entries, and a charge-only AR ledger
(payments added in Phase 4). This phase brings it to real double-entry-bookkeeping depth.

Build these as independent CQRS slices under the existing Finance module, following the Account/
JournalEntry pattern already in place:
1. Accounts Payable — mirror of the AR pattern (bill recorded on Goods Receipt or Supplier Invoice,
   payment recorded against it, outstanding-balance query).
2. Bank reconciliation — a BankStatementLine import (CSV) + a matching UI against JournalEntry
   lines, with a reconciled/unreconciled status per line.
3. Multi-currency — add a Currency value object and an exchange-rate table; JournalEntry lines
   carry both transaction-currency and functional-currency amounts.
4. Budgeting — a Budget aggregate per Account per period, with actual-vs-budget variance queries.
5. Fixed assets — an Asset aggregate with acquisition cost, depreciation method, and a scheduled
   depreciation JournalEntry generator.
6. Cost centers — a CostCenter dimension addable to JournalEntry lines for departmental reporting.
7. Financial statements — P&L and Balance Sheet as read-only aggregation queries over the Chart of
   Accounts + Journal Entries (no new persisted entity, pure reporting).
8. (Blocked on your decision) Tax engine — once you specify the jurisdiction(s), add a TaxRate
   aggregate and wire it into Sales Invoice / Purchase Order line calculation.

Verify: `tsc -b --force` clean for every new frontend page; each new aggregate has at least a
Create/List/GetById following the established permission-catalog convention.
```

---

## Phase 9 — Inventory costing + WMS depth

**Prerequisites:** Phase 0.
**Blocked on:** The inventory-valuation sub-step is blocked until you tell me the costing method to use (FIFO / weighted-average / standard cost) — everything else in this phase does not depend on that decision.

```
Context: Inventory has Product CRUD and a basic stock ledger; Warehouse has Warehouse/Zone CRUD and
Goods Receipt. Neither has the depth a real WMS needs — no batch/serial tracking, no barcode, no
transfers, no reservations, no picking/packing, and no costing.

Inventory side:
1. Variants/attributes — allow a Product to have variant dimensions (size/color/etc.) with
   per-variant SKUs.
2. Batch/lot tracking — a Batch entity linked to stock-ledger entries, with expiry-date support.
3. Serial number tracking — a Serial entity for uniquely-tracked units (alternative to Batch, not
   both on the same product).
4. Barcode/QR — generate a barcode/QR value per SKU (or per Batch/Serial) and expose it on the
   product detail; scanning is a frontend concern (camera input) layered on top later, not required
   this phase.
5. Multi-UOM conversion — a UnitOfMeasure conversion table so a Product can be stocked in one UOM
   and sold in another.
6. Stock transfers — a StockTransfer aggregate moving quantity between two warehouses, with an
   in-transit status.
7. Reservations — a soft-allocation of on-hand stock to a specific Sales Order, decremented from
   "available to promise" without touching physical on-hand until Dispatch actually occurs.
8. Cycle counting — a scheduled/ad-hoc count session comparing system quantity to counted quantity,
   generating an adjustment.
9. (Blocked on your decision) Inventory valuation — once you specify FIFO/weighted-average/
   standard, implement the costing layer so stock-ledger entries carry a real unit cost usable by
   Finance's stock-valuation report (Phase 6) and COGS on Sales Invoice.

Warehouse side:
10. Picking — a pick-list generated from a confirmed Sales Order / Dispatch, assignable to a user.
11. Packing — a pack confirmation step after picking, before Dispatch is marked shipped.
12. Putaway — a suggested/confirmed putaway location on Goods Receipt.
13. Bin-level location tracking — Zones get a Bin sub-entity; stock-ledger entries can reference a
    specific bin, not just a warehouse/zone.
14. Cycle counting (warehouse side) — same concept as Inventory's, scoped to a warehouse/zone/bin.

Verify: `tsc -b --force` clean; confirm (via code review, since no compiler is available here) that
Reservations correctly reduce available-to-promise without touching physical on-hand, since this is
the single easiest rule to get subtly wrong.
```

---

## Phase 10 — Procurement + Sales depth

**Prerequisites:** Phase 0.

```
Context: both modules currently cover only the "happy path" core transaction (PO create+approve;
Sales Order → Invoice → Dispatch). This phase adds the surrounding business processes real
customers expect.

Procurement:
1. RFQ (Request for Quotation) — a pre-PO stage where multiple suppliers are asked to quote; the
   winning quote converts into a PO.
2. Supplier scorecards — track on-time-delivery rate and quality-rejection rate per supplier,
   computed from Goods Receipt history.
3. Contract management — a Contract aggregate linking a Supplier to negotiated pricing/terms,
   referenced when creating a PO.
4. Three-way match — before a Supplier Invoice can be posted to Finance, validate its lines against
   the matching PO and Goods Receipt quantities/prices.
5. Multi-level approval matrix — extend Phase 7's generic Workflow engine with configurable
   approval thresholds (e.g. POs over $10,000 need a second approver).
6. Vendor returns — a return-to-supplier flow that reverses a Goods Receipt's stock and generates a
   debit note.
7. Idempotency keys on Create commands — add an idempotency-key header check to CreatePurchaseOrder
   (and CreateSalesOrder on the Sales side) to prevent duplicate-submission from a double-click or
   network retry.

Sales:
8. Quotations — a pre-Sales-Order stage, convertible into a real Sales Order once accepted.
9. Returns/credit notes — a return-from-customer flow that reverses a Dispatch/Invoice and issues a
   credit note against the customer's AR balance.
10. Pricing/discount engine — multiple price lists per customer segment, and a discount that can be
    applied at the Sales Order line level, validated against an approval threshold if it exceeds a
    configurable percentage.
11. Sales commissions — a commission-rate-per-salesperson calculation on invoiced (not just
    ordered) revenue.
12. Backorder handling — when a Sales Order line exceeds available-to-promise (Phase 9's
    reservations), allow a partial confirm with the remainder flagged as backordered.

Verify: `tsc -b --force` clean; three-way match and vendor-returns both correctly leave the
existing GoodsReceipt→Inventory ledger event chain intact (do not bypass the outbox/consumer
pattern already in place).
```

---

## Phase 11 — Document Management

**Prerequisites:** Phase 0.
**Blocked on:** New module — do not start without your go-ahead, since it wasn't in the original module set (only added when this roadmap was written).

```
Context: zero code exists anywhere for file attachments, blob storage, or document handling —
confirmed by a repo-wide grep in this session. This is a new cross-cutting module.

1. Design a `Document` aggregate (EntityType, EntityId, FileName, ContentType, SizeBytes,
   StorageKey, Version, UploadedBy, UploadedAt) attachable to any entity in any module (Product,
   PurchaseOrder, SalesInvoice, etc.) via the EntityType/EntityId pair — do not create a
   per-module Document table.
2. Blob storage integration — abstract behind an `IBlobStorage` interface with a local-disk
   implementation for now (S3/Azure Blob can be swapped in later without touching callers).
3. Document versioning — re-uploading against the same EntityType/EntityId keeps prior versions
   retrievable, not overwritten.
4. Access control — gate document read/upload/delete through the existing RBAC pipeline; add
   `core.document.read`/`core.document.upload`/`core.document.delete` (delete here means removing
   an attachment, which is a legitimate hard action unlike entity soft-deactivation — confirm this
   distinction explicitly in the PR/commit message so it isn't mistaken for a convention violation).
5. Frontend — a reusable `<DocumentAttachments entityType entityId />` component, dropped into the
   Product detail, PO detail, and Invoice detail pages first (expand to others afterward).

Verify: `tsc -b --force` clean; confirm the component is generic enough to be reused across
entities without a new component per module.
```

---

## Phase 12 — API Platform hardening

**Prerequisites:** Phase 0.
**Blocked on:** Needs your go-ahead — this expands the API surface FusionOS exposes externally, which has security implications worth a deliberate decision.

```
Context: Swagger/OpenAPI generation is real but Development-only; there are no API keys, no
developer portal, no outbound webhooks, and no per-partner rate limiting anywhere in the codebase
(confirmed this session).

1. API key management — an ApiKey aggregate (per company, scoped to specific permission codes,
   revocable) and an authentication scheme that accepts a key alongside the existing JWT scheme.
2. Per-key rate limiting — extend the existing rate-limiter (already used for IP-based limits per
   Phase C16/C17) to also key off ApiKey, with configurable quotas per key.
3. Developer portal — a minimal published-docs page (can be as simple as exposing the existing
   Swagger JSON publicly with a curated subset of endpoints, or a dedicated static docs site).
4. Outbound webhooks — a WebhookSubscription aggregate (company, event type, target URL, secret)
   and a dispatcher that POSTs a signed payload when subscribed events fire (reuse the existing
   outbox pattern — this is a natural extension of it, not a new delivery mechanism).

Verify: confirm the new ApiKey auth scheme does not weaken the existing JWT-based
tenant-isolation/authorization pipeline — an API key must still resolve to a specific CompanyId and
be subject to the same `AuthorizationBehavior`/`TenantIsolationBehavior` checks as a JWT-authenticated
user.
```

---

## Phase 13 (F-1) — Manufacturing

**Prerequisites:** Phase 0, Phase 9 (needs Inventory's batch/serial + costing depth to be meaningful).
**Blocked on:** PARKED per your standing instruction from earlier in this engagement — do not start this phase without an explicit, separate go-ahead from you, even though a plan now exists for it.

```
Context: Manufacturing is currently an empty scaffold (DbContext + health check returning
"scaffolded" only) — 0 real domain logic.

When greenlit, design and build, in this order: (1) Bill of Materials — single-level first (a
Product composed of component Products + quantities), then extend to multi-level/alternative BOMs;
(2) Work Orders — a production run against a BOM, consuming component stock and producing finished
-good stock through the same stock-ledger mechanism Inventory already uses (reuse, do not
duplicate, the existing GoodsReceiptPosted-style event pattern); (3) Routing — the sequence of
operations a work order passes through; (4) MRP — a planning run that looks at open Sales Orders +
current stock + BOMs and suggests Purchase Orders / Work Orders to cover the gap; (5) shop-floor
tracking — status updates against a work order's operations; (6) costing roll-up — a finished
good's cost computed from its BOM's component costs (depends on Phase 9's costing-method decision
being in place).

Follow the same Clean Architecture + CQRS + PermissionCatalog + soft-deactivate conventions as
every other module. Verify with `tsc -b --force` for any frontend work.
```

---

## Phase 14 (F-2) — CRM + HRMS

**Prerequisites:** Phase 0, Sales module (CRM needs Customer), Core module (HRMS needs Users/Departments).
**Blocked on:** PARKED per your standing instruction from earlier in this engagement — do not start this phase without an explicit, separate go-ahead from you, even though a plan now exists for it.

```
Context: both are empty scaffolds — 0 real domain logic in either.

CRM, when greenlit: Lead aggregate (pre-Customer prospect) → Opportunity (pipeline stage tracking)
→ conversion into a real Sales `Customer` once won. Contacts as a superset of Sales' Customer
concept (a Customer can have multiple Contacts). Activities/follow-ups as a simple timestamped log
against a Lead/Opportunity/Contact. Reporting: pipeline-by-stage and win-rate queries.

HRMS, when greenlit: Employee aggregate (linked 1:1 to a Core `User` where the employee also has
system access, but standalone otherwise) with Department/Branch assignment reusing Core's existing
Department/Branch entities (do not create parallel org-structure entities). Leave requests
(request/approve, reusing Phase 7's Workflow engine). Attendance (clock-in/out or a simpler daily
record). Payroll and performance reviews are the deepest, most business-rule-heavy parts — scope
them as their own sub-phase once the above is working, since payroll correctness (tax withholding,
statutory compliance) is jurisdiction-specific and will need the same kind of explicit decision
Finance's tax engine needed.

Verify with `tsc -b --force` for any frontend work; confirm HRMS's Employee-to-User link does not
duplicate or conflict with Core's existing User/Department/Branch entities.
```

---

## Phase 15 (F-3) — Quality + Maintenance

**Prerequisites:** Phase 0, Phase 9 (Quality's Goods Receipt checkpoint), Phase 13 (Quality's production checkpoint, Maintenance's equipment-in-Manufacturing linkage).
**Blocked on:** PARKED per your standing instruction from earlier in this engagement — do not start this phase without an explicit, separate go-ahead from you, even though a plan now exists for it.

```
Context: both are empty scaffolds.

Quality, when greenlit: Inspection Plan (a checklist template tied to a Product or BOM operation) →
QC Checkpoint results recorded against a specific Goods Receipt line or Manufacturing work-order
operation → Non-Conformance record when a checkpoint fails → CAPA (Corrective/Preventive Action)
tracking against a Non-Conformance → Certificate of Analysis generation for a passed batch.

Maintenance, when greenlit: Asset/Equipment registry (can reference Warehouse locations and,
later, Manufacturing's production equipment) → Preventive Maintenance schedule (time- or
usage-based) → Work Orders (maintenance-specific, distinct from Manufacturing's production work
orders, though both could eventually share the Workflow engine's approval mechanics) → downtime
tracking → spare-parts inventory linkage (consumes Inventory stock the same way Manufacturing's
BOM consumption does).

Verify with `tsc -b --force` for any frontend work.
```

---

## Phase 16 (F-4) — Integration Hub + Marketplace

**Prerequisites:** Phase 0, Phase 12 (Integration Hub needs a hardened API Platform to integrate through).
**Blocked on:** PARKED per your standing instruction from earlier in this engagement — do not start this phase without an explicit, separate go-ahead from you, even though a plan now exists for it.

```
Context: both are empty scaffolds.

Integration Hub, when greenlit: builds directly on Phase 12's outbound-webhook infrastructure —
add an inbound-connector framework (a generic adapter interface so a new e-commerce/payment/
shipping integration is a matter of implementing one interface, not building bespoke plumbing per
partner), then the first 1-2 real connectors (whichever partner you actually need first — do not
build speculative connectors with no real target).

Marketplace, when greenlit: multi-vendor product listings (a Vendor concept distinct from
Procurement's Supplier — a Vendor sells *through* FusionOS rather than *to* it), vendor onboarding,
a commission/payout engine (reuses Finance's AP-style payment recording pattern from Phase 8), and
a storefront (likely a separate frontend surface, not the internal ERP UI — scope this explicitly
before starting, since it has very different UX requirements than the rest of FusionOS).

Verify with `tsc -b --force` for any frontend work.
```

---

## Phase 17 (F-5) — Mobile + Analytics

**Prerequisites:** Phase 0, Phase 12 (Mobile needs a stable, versioned, rate-limited API to build against).
**Blocked on:** PARKED per your standing instruction from earlier in this engagement — do not start this phase without an explicit, separate go-ahead from you, even though a plan now exists for it.

```
Context: Mobile has no folder at all (not even a scaffold); Analytics is an empty scaffold.

Mobile, when greenlit: decide the platform approach first (React Native reusing frontend logic vs.
a fully separate native app) — this is a real architectural decision, not just an implementation
detail, and should get its own short design doc before code starts. First real app: a Warehouse
picker app covering Phase 9's picking/packing workflow, since that is the single most
mobile-native workflow in the whole system (a person walking a warehouse floor, not sitting at a
desk). A Sales-rep app (quotes/orders on the go) would be the second target.

Analytics, when greenlit: a cross-module analytics store (likely a read-replica or a
purpose-built reporting schema, not queries against the live transactional tables) feeding trend
dashboards and cohort/retention analysis; export to an external BI tool (Metabase/PowerBI/Looker)
as the integration point rather than building a BI tool from scratch.

Verify with `tsc -b --force` for any frontend/web work; mobile verification will need its own
platform-specific tooling not yet present in this environment.
```

---

## Phase 18 (F-6) — AI + SAP Migration

**Prerequisites:** Phase 0, and meaningfully all of Phases 1–17 — both need mature, high-volume real data across the whole system to be worth building.
**Blocked on:** PARKED per your standing instruction from earlier in this engagement — do not start this phase without an explicit, separate go-ahead from you, even though a plan now exists for it.

```
Context: AI is an empty scaffold with zero AI/ML/LLM-backed code anywhere in the repo; SAP
Migration has no folder at all.

AI, when greenlit: do not start with a generic "AI module" — pick one concrete, high-value use
case first (demand forecasting off Sales Order history, or anomaly detection on Finance journal
entries are the two most defensible starting points given what data will actually exist by this
phase) and build that end-to-end before generalizing. Natural-language query and document/OCR
extraction are reasonable later additions once one real use case is proven.

SAP Migration, when greenlit: data-mapping tooling (a configurable field-mapping layer from SAP's
schema to FusionOS's, likely starting with Finance's Chart of Accounts and Inventory's Product
master, since those are the two most-mapped entities in any ERP migration), an IDoc/RFC connector
(or a simpler CSV/staging-table import if a live SAP connection isn't available for testing),
and migration validation/reconciliation reports (row counts and checksum-style comparisons between
source and migrated data).

Both of these should be scoped with a dedicated short discovery/design pass before implementation,
since "AI" and "SAP Migration" are the two vaguest, least-concretely-specified items in the entire
roadmap — more planning is needed at that time, not just code.
```

---

## How to track progress against this plan

After each phase, update `docs/PROJECT_TRACKER.md`'s checklist and re-run the module completion percentages in `docs/DEVELOPMENT_ROADMAP.md` Step 3/9 so the dashboard stays honest. Do not bump a percentage without pointing at the specific file/endpoint/test that justifies the new number — the standing rule for this whole engagement is verify, don't estimate.

*Built from `docs/DEVELOPMENT_ROADMAP.md` (2026-07-14/15). No source files were created or modified to produce this plan itself — it is a planning document, same as the roadmap it's derived from.*
