# FusionOS Remediation Roadmap (post Sprint Audit, 2026-07-14)

Continues the existing Phase A–E numbering from `README.md`. Phase F (Manufacturing, CRM, HRMS,
AI, SAP migration) stays explicitly out of scope, as decided earlier — it's listed at the end for
completeness only. Phases below are ordered by dependency, not just priority: each one assumes the
ones before it are done, so don't skip ahead to Phase K tests if Phase G's migrations don't exist
yet.

## Progress since this roadmap was written

- **Phase G is partially unblocked**: a real .NET 8 SDK now runs in this sandbox (downloaded via
  `apt-get download` + `dpkg-deb -x` into a native, non-mounted directory — no root needed).
  `dotnet --version`/`--list-sdks` work for real. **However, `dotnet restore` is still blocked**:
  `api.nuget.org` and every NuGet CDN endpoint return `403` from this sandbox's network allowlist
  proxy. FusionOS's actual dependencies (MediatR, EF Core, Serilog, etc.) can never be restored
  here, so generating real migrations and running a full build/test still requires your own
  machine. This is a harder, different blocker than "no SDK" — confirmed, not assumed.
- **Phase H1 is done**: all 16 List/Get query handlers that had zero permission gating now require
  a `*.read` permission (`finance.account.read`, `inventory.product.read`, etc. — 13 new codes
  added to `PermissionCatalog.cs`). Since the existing "Owner" role auto-grants every catalog
  permission, this needed no data migration and doesn't break any existing user. Verified by
  static analysis only (brace-balance + structural review) — not compiled, per the NuGet blocker
  above.
- **Phase H2–H5 and Phase I are done; Phase J is partially done** (same day, follow-up pass, run
  via four parallel sub-agents partitioned by module boundary to avoid file collisions): RBAC
  administration (create roles, edit permissions, assign users) shipped end-to-end including the
  frontend page at `/core/roles`; new user registrations into an *existing* company no longer
  auto-grant the all-permissions "Owner" role (only a brand-new company's first user does; everyone
  else gets a zero-permission "Member" role an Owner must explicitly promote); a real audit-log
  read side exists at `/core/audit-log`; a public `/register` page exists. Separately, every entity
  that had a dead `GetById` stub (Company, Product, Warehouse, Zone, Supplier, Account, Customer)
  now has a real GetById, an Update command, and a soft Deactivate command (never a hard delete) —
  16 new permission codes were added for these. Two Phase J items landed alongside this: a
  maker-checker fix so a purchase order can't be self-approved by its own creator, and a missing
  EF Core decimal-precision fix on `InvoiceLine`/`DispatchLine`. **Not done**: Sales
  cross-aggregate quantity validation (nothing yet stops over-invoicing/over-dispatching beyond
  what a SalesOrder actually ordered — scoped out as too risky to rush without a compiler) and a
  newly-surfaced gap where `InvalidOperationException`/`KeyNotFoundException` fall through to a
  generic 500 instead of 409/404 in the shared exception handler. Verified by a full
  `tsc -b --force` pass (0 errors, real execution) on the frontend and an independent byte/brace/
  paren integrity sweep across all 104 touched files (0 flagged) — backend C# itself is still
  static-analysis-only, since `dotnet build` remains blocked here.

---

| Phase | Focus | Duration | Blocking on |
|---|---|---|---|
| G | Unblock deployment | ~1 week | Needs your own machine — confirmed blocked here by network policy, not just a missing SDK |
| H | Close RBAC/security gaps | ~1.5–2 weeks | **Done** (H1–H5 all complete) |
| I | Mechanical correctness sweep | ~1–1.5 weeks | **Done** |
| J | Make cross-module events real | ~1 week | Partially done — self-approval + decimal-precision fixed; event wiring/AR/cross-validation remain |
| K | Raise test/CI confidence | ~1.5–2 weeks | Phase G (needs a real schema to test against) |
| L | Baseline enterprise features | ~6–10 weeks | Phases G–K; several items blocked on business decisions |
| F | Manufacturing/CRM/HRMS/AI/SAP/Mobile | Months | Explicitly deferred — not started unless you say so |

---

## Phase G — Unblock deployment (Critical)

Nothing past this phase matters if the app can't stand up a database. This has to happen on a
real machine, not in this sandbox — confirmed twice now: no dotnet SDK originally, and now that
one exists, no network path to NuGet either.

1. **Generate and commit real EF Core migrations** for all 6 implemented modules
   (`./scripts/generate-migrations.sh`, then review and commit the output). 2–3 days.
2. **Apply migrations in CI** before backend tests run — right now the CI Postgres container gets
   provisioned but never migrated, so the one real integration test suite likely fails. 1 day.
3. **Verify the fixes made by static analysis only** (auth bootstrap `[AllowAnonymous]`,
   `GET /companies` tenant scoping, the 16 read-permission gates, and everything added in the
   Phase H/I/J follow-up pass) with an actual `dotnet build && dotnet run` and a real
   create-company → register → login round trip. A few hours.
4. **Run a real backup/restore drill** against `scripts/backup-postgres.sh`/`restore-postgres.sh`
   and record actual RPO/RTO numbers in `docs/DISASTER_RECOVERY.md` — currently unverified.
   Half a day.

**Exit criteria:** a fresh clone can `docker compose up`, migrate, register a company, log in, and
CI passes end-to-end.

---

## Phase H — Close RBAC and security gaps (Critical)

The biggest gap the audit found isn't a missing feature, it's that every registered user is an
unrestricted superuser. This phase fixes that before more code gets built on top of it.

1. ~~**Gate RBAC on reads, not just writes**~~ — **done.** All 16 query handlers now require a
   `*.read` permission; `PermissionCatalog.cs` has the 13 new codes.
2. ~~**Build RBAC administration**~~ — **done.** Create role, list roles, get/set role permissions,
   list company users, assign user-to-role — full API + `/core/roles` UI.
3. ~~**Stop auto-granting every new user the all-permissions "Owner" role**~~ — **done.** Only a
   brand-new company's first registrant becomes Owner; every later registration into an existing
   company gets a zero-permission "Member" role instead.
4. ~~**Build the audit-trail read side**~~ — **done.** `ListAuditLogEntriesQuery` +
   `AuditLogController` + `/core/audit-log` page. Real before/after diff capture (`ChangesJson`)
   is still hardcoded `null` — that part remains open.
5. ~~**Frontend registration/sign-up page**~~ — **done.** `/register`, linked from `/login`.

**Exit criteria:** a non-Owner role can exist, reads are permission-checked *and* enforced against
a real second role, and there's a UI to manage all of it. **Met**, pending Phase G's real
compile/run verification.

---

## Phase I — Mechanical correctness sweep (High)

These are repo-wide copy-pasted patterns, not one-off bugs — worth fixing as a single batch pass
rather than piecemeal.

1. ~~**Fix dead `GetById` stubs**~~ — **done**, for Companies, Products, Warehouses, Zones,
   Suppliers, Customers, and (bonus) Accounts. Each also fixed the broken `CreatedAtAction` target
   for its own `Create` endpoint.
2. ~~**Add Update/Deactivate endpoints**~~ — **done**, for the same 7 entities. Implemented as a
   soft `Deactivate()` (never a hard delete), matching the domain-level pattern that already
   existed as dead code on `Company`.
3. **Add idempotency keys** to create-commands (Goods Receipt, Purchase Order, etc.) to stop
   double-submit duplication — **still open**. 2–3 days.
4. ~~**Fix missing decimal-precision EF config**~~ on `InvoiceLine`/`DispatchLine` — **done**
   (`numeric(19,4)`, matching `JournalEntryLine`/`SalesOrderLine`/`PurchaseOrderLine`).

**Exit criteria:** every entity has a full CRUD lifecycle; no controller returns a hardcoded stub.
**Met** for the 7 entities above.

---

## Phase J — Make cross-module events real (High)

Several domain events are raised into Kafka and never consumed — the plumbing works, nothing's
listening.

1. **Wire or retire orphaned events**: `SalesOrderConfirmed`, `DispatchLineDispatched`,
   `CompanyCreated`, `WarehouseCreated`, `ZoneCreated`. 3–5 days. **Still open.**
2. **Build AR collections/payments** so Finance's customer balance reflects real outstanding AR
   instead of lifetime-invoiced total (currently charge-only, can never decrease). 3–5 days.
   **Still open.**
3. **Add cross-aggregate validation**: Invoice/Dispatch quantities against the parent Sales
   Order's remaining quantity, closing the double-invoice/over-dispatch gap. 2–3 days. **Still
   open** — investigated and scoped (needs a per-product "already consumed" sum on both
   `IInvoiceRepository` and `IDispatchRepository`), deliberately not implemented without a
   compiler to verify it.
4. ~~**Fix Purchase Order self-approval**~~ — **done.** `ApprovePurchaseOrderCommandHandler` now
   rejects `_currentUser.UserId == order.CreatedBy` with `InvalidOperationException`. **Caveat**:
   the shared exception handler doesn't map `InvalidOperationException` to 409 yet (see new gap
   below) — it'll surface as a 500 until that's fixed.

**New gap surfaced by item 4**: `ProblemDetailsExceptionHandler` only maps `ValidationException`
→400 and `ForbiddenException`→403; `InvalidOperationException` (used by this fix and by
`JournalEntry.Post()`'s own double-post guard) and `KeyNotFoundException` (not-found lookups) both
fall through to a generic 500. Pre-existing, not introduced by this fix, but now more visible.
Add explicit mappings (`InvalidOperationException`→409, `KeyNotFoundException`→404). Half a day.

**Exit criteria:** every raised domain event has a real consumer or is removed; AR reflects
reality; basic segregation of duties exists on approvals. **Partially met** — segregation of
duties (item 4) is done; the rest of the phase remains.

---

## Phase K — Raise test and CI confidence (High)

Only ~31% of implemented handlers have any test, 0% of queries, and the one integration-test
suite likely fails today for lack of a migrated schema (Phase G fixes that part). The Phase H/I/J
follow-up pass added roughly 60 new command/query handlers with zero test coverage of their own —
this phase's scope grew accordingly.

1. **Test every untested command/query handler** (ApprovePurchaseOrder, ConfirmSalesOrder,
   CreateGoodsReceipt, CreateInvoice, IssueInvoice, CreateDispatch, all 17 original query handlers,
   and every new GetById/Update/Deactivate handler added across Core/Inventory/Warehouse/
   Procurement/Finance/Sales). 2–3 weeks (revised up from 1–2).
2. **Test every cross-module integration-event consumer** — these are the most fragile code in
   the repo and currently have zero coverage. 3–5 days.
3. **Add a coverage gate and basic dependency/SAST scanning to CI** (CodeQL or `dotnet list
   package --vulnerable` + `npm audit`, at minimum). 2–3 days.

**Exit criteria:** every handler and consumer has at least one test; CI fails the build if
coverage drops or a known-vulnerable package is introduced.

---

## Phase L — Baseline enterprise features (Medium)

This is the largest phase and the one most likely to need your input along the way — several
items are flagged as blocked on a business decision, not an engineering one.

| Item | Estimate | Notes |
|---|---|---|
| Settings module | 1 week | Zero code exists today. |
| Unified platform-wide search | 1–2 weeks | Currently 5 of 20+ entities have narrow `ILIKE` search. |
| Notifications (real email/SMS/push) | 1–2 weeks | **Blocked**: needs a provider choice (SendGrid/Twilio/SES/etc.) and likely a paid account. |
| Basic Reports module | 1–2 weeks | Nothing exists; even CSV/PDF export closes a real gap. |
| Generic Workflow/Approval engine | 2–3 weeks | Replaces the single-step `Approve()` pattern with real multi-level approval. |
| Inventory costing + batch/serial + multi-UOM | 3–4 weeks | **Blocked**: needs a decision on FIFO vs weighted-average vs standard costing. |
| Warehouse WMS depth (picking/packing/putaway/bins/cycle count) | 4–6 weeks | Can be phased — picking+packing first. |
| Finance depth (AP, bank reconciliation, financial statements, multi-currency, budgeting, fixed assets, cost centers) | 6–8 weeks | **Partly blocked**: GST/tax engine needs a jurisdiction decision. |
| Procurement depth (RFQ, supplier scorecards, contracts, three-way match) | 3–4 weeks | Three-way match needs Finance's AP (above) to exist first. |
| Sales depth (returns/credit notes, pricing/discount engine, quotations, commissions, backorders) | 3–4 weeks | — |

**Exit criteria:** each of the 6 real modules has the feature depth expected of a baseline
(not full-SAP-grade) ERP, and the 3 blocked items have a decision recorded.

---

## Phase F — Deferred (unchanged from your earlier decision)

Manufacturing (BOM/routing/work orders/MRP/scheduling), CRM, HRMS, Quality, Maintenance, Business
Intelligence, Marketplace, Integration Hub, AI platform, SAP migration tooling, Mobile apps. Each
is a multi-month workstream on its own. Not started unless you explicitly greenlight it — flagged
here only so the roadmap is complete, not as a proposal to begin.

---

## Decisions needed from you before Phase L can finish

1. **Inventory costing method** — FIFO, weighted-average, standard, or a per-product choice.
2. **Tax jurisdiction(s)** — GST (India) only, VAT, multi-country, or configurable.
3. **Notification provider** — SendGrid, Twilio, SES, or other, plus who provisions the account.

Everything else in Phases G–K is a pure engineering decision and can proceed without waiting on you.
