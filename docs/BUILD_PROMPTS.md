# FusionOS — Phase-Wise Build Prompts (post Coverage & Completion Audit, 2026-07-14)

Each phase below is a self-contained prompt you can paste as-is into a new Cowork/Claude session
to continue the build. They're ordered by dependency and priority, matching
`docs/REMEDIATION_ROADMAP.md` and `FusionOS_Coverage_Completion_Audit.docx`. Run them one at a
time, in order, within this same sandbox — except **Phase G**, which needs to run on your own
machine (see the callout below), and **Phase F**, which stays parked until you explicitly say go.

Each prompt is written to be understandable without any other context — paste it fresh, or paste
it into a continuation of this conversation, either works.

---

## Phase G — Unblock deployment (run this one on your OWN machine, not in Cowork)

This is the one phase that cannot happen in this sandbox — no NuGet network access here, confirmed
twice. Once you're on a machine with Docker Desktop + the .NET 8 SDK + Node 20 installed (see the
"Running it" section of `README.md`), open Claude Code or Cowork *pointed at that machine* and use
this prompt:

```
Run ./scripts/generate-migrations.sh in this FusionOS repo, then apply the migrations with
./scripts/generate-migrations.sh --apply (or the dotnet ef database update commands it prints).
Then run dotnet build on the whole solution and dotnet test on every test project. Fix any real
compiler errors or failing tests you find — this codebase has never been compiled before, so
expect genuine issues, not just formatting nits. Once it builds and tests pass, run
docker compose up --build, register a company, log in, and confirm the basic create/list flows
work end to end on at least Companies, Products, and Sales Orders. Report exactly what you had to
fix.
```

---

## Phase M1 — Quick correctness fixes (do this first, it's small and unblocks nothing else)

```
Two known, isolated bugs need fixing in FusionOS, both flagged by the latest coverage audit
(FusionOS_Coverage_Completion_Audit.docx, Section 11 Critical Blockers):

1. ProblemDetailsExceptionHandler (find it under backend/src/Host or BuildingBlocks — grep for the
   class) only maps ValidationException->400 and ForbiddenException->403. Add mappings:
   InvalidOperationException -> 409 Conflict, KeyNotFoundException -> 404 Not Found. This matters
   right now because ApprovePurchaseOrderCommandHandler's self-approval rejection
   (InvalidOperationException) currently surfaces as a raw 500 instead of a 409.

2. Sales cross-aggregate quantity validation is missing: CreateInvoiceCommandHandler and
   CreateDispatchCommandHandler (backend/src/Modules/Sales/FusionOS.Modules.Sales.Application/)
   fetch the SalesOrder only to check it exists, never compare line quantities against what was
   actually ordered or already invoiced/dispatched. Add: a method on IInvoiceRepository and
   IDispatchRepository to sum previously-consumed quantity per ProductId for a given SalesOrderId,
   then reject a request line that would exceed the SalesOrder line's remaining quantity (throw
   ValidationException, matching the existing "sales order does not exist" pattern in the same
   handlers).

Follow the same file-integrity discipline as the rest of this project: after every Write/Edit,
verify via a bash null-byte/brace/paren check (the mount has a known silent-truncation bug), and
rewrite via heredoc if anything looks off. No compiler is available here, so verify by careful
reading against existing patterns, not by building.
```

---

## Phase M2 — Frontend Update/Edit forms for the 7 entities that already have a working backend

```
The coverage audit found that Company, Product, Warehouse, Zone, Supplier, Account, and Customer
all have a fully-implemented backend Update command (real handler, validator, permission gate) but
zero frontend caller — no edit form exists anywhere, so these commands are functionally dead from
the UI's perspective. Only Deactivate got a button; Update did not.

For each of the 7 pages (CompaniesPage.tsx, ProductsPage.tsx, WarehousesPage.tsx, ZonesPanel.tsx,
SuppliersPage.tsx, AccountsPage.tsx, CustomersPage.tsx — all under frontend/src/modules/), add an
"Edit" button per row that opens an inline form (reuse the same react-hook-form + zod pattern
already used for each page's Create form) pre-filled from the row's data, calling
apiClient.put(...) to the entity's existing PUT endpoint. Use RolesPage.tsx's
RolePermissionsEditor as a reference for how this codebase does an inline edit panel next to a
list. Use four parallel sub-agents partitioned by module (Core; Inventory+Warehouse;
Procurement+Finance; Sales) to avoid file collisions, exactly like the pattern used for the
GetById/Update/Deactivate backend sweep — each agent should follow the same mandatory
byte/brace/paren verification after every file write given the mount's known truncation bug, and
must not touch AppRoutes.tsx, PermissionCatalog.cs, or client.ts. Run a full frontend
`tsc -b --force` afterward and report 0 errors before considering this done.
```

---

## Phase M3 — Test coverage for the ~60 untested handlers from the last build session

```
The coverage audit confirmed zero test coverage for everything added in the RBAC administration +
Phase I mechanical sweep: CreateRoleCommandHandler, SetRolePermissionsCommandHandler,
AssignUserRoleCommandHandler, ListAuditLogEntriesQueryHandler, the RegisterUserCommandHandler
stop-auto-Owner branch, and all of GetById/Update/Deactivate for Company, Product, Warehouse,
Zone, Supplier, Account, and Customer (21 handlers across those 7 entities alone).

Add unit tests for each, following the existing style in backend/tests/ (NSubstitute-mocked
repository + IUnitOfWork, one happy-path test and one failure-path test per handler at minimum —
see CreateCompanyCommandHandlerTests.cs and PostJournalEntryCommandHandlerTests.cs as the
reference pattern for both). Use parallel sub-agents partitioned by module the same way as prior
phases. No compiler is available in this sandbox, so these tests cannot be run here — write them
carefully against the actual handler signatures (read each handler file directly, don't guess),
and flag this clearly: these tests are unverified until Phase G runs `dotnet test` on a real
machine.
```

---

## Phase M4 — Wire or retire orphaned domain events, add AR payments

```
The coverage audit found roughly 11 of 14 domain events raised across FusionOS are never consumed:
CompanyCreated, WarehouseCreated, ZoneCreated, SupplierCreated, CustomerCreated, AccountCreated,
SalesOrderCreated, SalesOrderConfirmed, InvoiceCreated, JournalEntryCreated, JournalEntryPosted,
PurchaseOrderCreated, PurchaseOrderApproved. For each, either build a real consumer (if there's an
obvious cross-module use — e.g. JournalEntryPosted updating a cached account balance) or
explicitly document in the event's own file why it's intentionally a no-op for now. Don't leave
them silently unconsumed without a decision recorded either way.

Separately: build AR payment/receipt recording. Today ArLedgerEntry only supports positive
"charge" entries fed by InvoiceIssued — there's no way to record a customer payment, so the
balance can only ever increase. Add a RecordCustomerPaymentCommand (negative-amount ledger entry,
tenant-scoped, permission-gated with a new finance.receivable.record-payment code — report it,
don't add it to PermissionCatalog.cs yourself) and a frontend page for the AR balance/ledger, which
currently doesn't exist at all.
```

---

## Phase M5 — Settings module + real search

```
Two zero-code gaps confirmed by the coverage audit:

1. Settings module: nothing exists — no entity, no API, no frontend. Build a minimal Settings
   slice scoped to company-level configuration (start with just: default currency override,
   default page size, and a company display-name/logo placeholder — keep the first cut small).

2. Search: only 5 of 19 list endpoints (Account, Customer, Supplier, Warehouse, Product) have real
   server-side ILIKE search; Roles, Users, Audit Log, Companies, and Permissions have none, and
   EntityCombobox.tsx's own code comments admit it only does client-side filtering. Add
   server-side search to the remaining list query handlers following the exact pattern already
   used in ListProductsQueryHandler/ListSupplierQueryHandler (grep them for the reference), and
   wire it into the corresponding list pages.
```

---

## Phase M6 — Reports + Dashboard

```
Neither a Reports module nor a Dashboard exists anywhere in FusionOS today (confirmed by the
coverage audit — zero code, not even a scaffold). Build a first, deliberately small version of
each:

- Reports: one generic "export this list to CSV" action reusable across the existing list
  endpoints (Products, Customers, Suppliers, Sales Orders, Invoices at minimum), plus a single
  canned report — Accounts Receivable aging — as the first real report.
- Dashboard: a single landing page (replace or extend ModuleHealthPage's role) showing a handful
  of real KPI numbers pulled from existing endpoints already built: open Sales Orders count,
  pending Purchase Order approvals count, low-stock product count (from the stock ledger), and
  today's Audit Log entry count. Don't invent new backend aggregation endpoints beyond what's
  strictly needed for these four numbers — reuse existing List/Get endpoints where their pagination
  metadata (totalCount) already gives you the number you need.
```

---

## Phase M7 — Generic Workflow/Approval engine + real Notification delivery

```
Two related gaps from the coverage audit:

1. There is no generic workflow/approval engine — only a single one-off
   `PurchaseOrder.Approve()` method with a maker-checker check. Design and build a small, reusable
   approval primitive (e.g. an `ApprovalRequest` aggregate: EntityType/EntityId/RequestedBy/
   Status/ApprovedBy, with a simple single-approver-not-equal-requester rule to start) that
   Purchase Order can be refactored onto, so the next module that needs approval (Sales Order
   discount override, Journal Entry over a threshold, etc.) doesn't reinvent it.

2. The Notification domain entity (backend/src/Modules/Core/FusionOS.Modules.Core.Domain/
   Notifications/Notification.cs) is fully dormant — persisted but with no command/handler, no
   controller, and no delivery mechanism. Before building real email/SMS/push delivery, this needs
   a decision from the user on a provider (SendGrid/Twilio/SES/other) — ask before implementing a
   specific integration. In the meantime, build the in-app half: a command to create a
   Notification, a query to list a user's unread notifications, and a small bell-icon UI, so the
   entity stops being unreachable even before an external provider is chosen.
```

---

## Phase M8 — Finance depth (partly blocked on a decision)

```
Before starting, ask the user: what tax jurisdiction(s) should FusionOS support (GST-India only,
VAT, multi-country, or fully configurable)? Don't guess — this changes the tax-engine design.

Once that's answered (or if deferred, build everything below except the tax engine): Finance
currently has only Chart of Accounts, Journal Entries, and a charge-only AR ledger — confirmed by
the coverage audit to have zero Accounts Payable, no bank reconciliation, no multi-currency, no
budgeting, no fixed-asset management, and no cost centers. Build these as separate vertical slices,
in this order: (1) Accounts Payable (mirror the AR ledger's shape — Supplier-facing instead of
Customer-facing), (2) cost centers (a lightweight dimension on Journal Entry lines), (3) bank
reconciliation, (4) fixed assets, (5) budgeting, (6) multi-currency, (7) the tax engine if a
jurisdiction was chosen above.
```

---

## Phase M9 — Inventory costing (blocked on a decision) + Warehouse WMS depth

```
Before starting inventory costing, ask the user: which costing method should FusionOS use — FIFO,
weighted-average, or standard costing (or should it be a per-product choice)? The stock ledger
today only has a raw per-transaction UnitCost snapshot, no real valuation method — don't guess
which one to build.

Once answered: build the costing layer on top of the existing InventoryLedgerEntry
(backend/src/Modules/Inventory/...), then add batch/lot tracking, serial-number tracking, and
multi-UOM conversion — all confirmed missing by the coverage audit.

Separately, and not blocked on anything: Warehouse has zero WMS depth today — no picking, packing,
putaway, bin-level locations, or cycle counting. Build picking + packing first (the roadmap's own
suggested order), as a workflow layered on top of the existing Dispatch aggregate rather than a
parallel system.
```

---

## Phase M10 — Procurement depth + Sales depth

```
Procurement gaps confirmed by the coverage audit: no RFQ (Request for Quotation) workflow, no
supplier scorecards/performance tracking, no contract management, no three-way match (PO / Goods
Receipt / Invoice), and only a single maker-checker check rather than a real multi-level approval
matrix (this last one should reuse the approval primitive from Phase M7 if that's already done).
Build RFQ first — it's the natural predecessor to a real Purchase Order in most procurement flows.

Sales gaps confirmed by the coverage audit: no returns/credit notes, no pricing/discount engine
(every line today uses a flat manually-entered unit price, no price lists or quantity breaks), no
quotations (pre-Sales-Order stage), no commission tracking, no backorder handling. Build returns
and the pricing engine first — those unblock the most real-world usage.
```

---

## Phase F — Deferred (do NOT start without an explicit go-ahead)

You've already decided to keep Manufacturing, BOM, MRP, CRM, HRMS, Quality, Maintenance, AI,
Marketplace, Integration Hub, Mobile Apps, and SAP Migration out of scope. Each is confirmed by the
coverage audit to be either an empty architectural scaffold or, for Mobile/SAP Migration, not to
exist as a folder at all. When and if you're ready to greenlight any one of these, here's a prompt
template — fill in the module name and don't run it until you mean it:

```
Design and build a real first vertical slice for the <MODULE NAME> module in FusionOS. It
currently exists only as an empty architectural scaffold (ModuleMarker.cs + an empty DbContext +
a health-check controller returning {"status": "scaffolded"}) — or, for Mobile/SAP Migration,
doesn't exist as a project at all yet. Read docs/blueprint/05_MODULE_ROADMAP.md and the relevant
blueprint doc for this module's intended scope first, then propose a plan for the first real slice
before writing any code, since this is a genuinely new module, not a continuation of existing work.
```

---

## Suggested order

M1 -> M2 -> M3 -> Phase G (on your machine, can happen in parallel with M2/M3 once you have time)
-> M4 -> M5 -> M6 -> M7 -> M8/M9/M10 (these three can interleave, each partly gated on a decision
from you) -> Phase F only if and when you say so.
