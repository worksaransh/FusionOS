# FusionOS — Master Future Build Plan

**Prepared:** 2026-07-17
**Based on:** a fresh full-repo audit (all 15 backend module folders read on disk, not from memory), cross-checked against `docs/PROJECT_TRACKER.md`, `docs/blueprint/05_MODULE_ROADMAP.md`, and `docs/ORPHANED_EVENTS_AUDIT.md`.
**Purpose:** one ordered, step-by-step plan for everything still pending, replacing the scattered "future follow-up" notes left across dozens of doc comments with a single sequenced roadmap. This does not repeat what `PROJECT_TRACKER.md` already records as done — it only covers what's left.

---

## 0. Governing principle for every step below

Every phase already built in this engagement (M1–M10) followed one discipline: small, narrowly-scoped vertical slices, each explicitly documenting what it deliberately does *not* do yet, reusing an existing aggregate's exact shape rather than inventing a new pattern. That discipline is now the standard, not optional, and it exists for a concrete reason: **fewer lines means fewer places for the filesystem mount-staleness bug and the missing-`using` bug to hide, and a smaller diff is what makes "static-analysis-only, never compiled" verification actually trustworthy.**

Concretely, for every future step:

- Copy the nearest existing analogous aggregate's file layout exactly (domain aggregate → CQRS commands/queries → repository → EF config → controller → tests → frontend panel). Do not invent a new project structure per feature.
- Prefer one well-placed guard clause or one repository method over a new abstraction layer. Every M8 slice added at most one or two new repository methods per aggregate — that ratio held because the logic was pushed into the aggregate's own methods (`Create`, `Reconcile`, `Dispose`), not spread across services.
- A deferred capability gets one doc-comment sentence, not a partial implementation. Half-wiring a feature (e.g. a `CostCenterId` column that nothing reads) is worse than not adding it, because it looks done in a `grep` and isn't.
- No phase below should touch a module it doesn't own. Cross-module reads happen through a repository interface + an injected read-only lookup, exactly like `RecordBillCharge` checks supplier existence and `CreateBudgetLine` checks account existence — never a foreign-key join across module boundaries.
- Every new command handler must import the correct module-canonical `IUnitOfWork` namespace (grep for this before calling any slice "done" — it has been the single most common real bug found this entire engagement, five separate times).

---

## Step 1 (prerequisite gate) — Phase G: make the codebase actually run

**Status: blocked on your own machine, unchanged since it was first flagged.** Nothing else on this list matters as much as this one, and no amount of further prompting resolves it — this sandbox has no .NET SDK network access and has never once compiled this repository.

What to do, in order, on your own machine (Docker Desktop + .NET 8 SDK + Node 20 installed — see `docs/BUILD_PROMPTS.md` for the exact copy-paste commands):

1. `dotnet restore` then `dotnet build FusionOS.sln` — fix whatever genuine compiler errors surface. Expect some; this codebase has 258 Finance files, 154 Sales files, and more, all written by careful reading, never compiled. A missing `using`, a namespace typo, or an EF configuration mismatch are the likely failure classes based on the two real bugs already found by manual brace-balance sweeps.
2. Generate the first-ever EF Core migration per module (`dotnet ef migrations add InitialCreate`) and apply it to a real Postgres instance (Docker Compose already defines one).
3. `dotnet test` across every test project — expect some tests to fail on first real run even though they were written correctly by inspection, since NSubstitute setups and FluentAssertions usage have never been checked by a real compiler either.
4. `npm run build` / `tsc -b` on the frontend against the now-real backend, and do one real end-to-end smoke pass: log in, create a company, walk through one flow per real module (e.g. create a Supplier → PO → Goods Receipt → Inventory ledger entry; create an Invoice → AR ledger; create a Cost Center → Budget → vs-actual).

Once this step is done, every `[x]` in `PROJECT_TRACKER.md` should be re-verified against a real test run rather than a reading — that re-verification pass itself is worth a tracker entry (call it Phase G-verify) before trusting percentages further.

---

## Step 2 — Close the integration gaps inside modules that already exist

These are not new modules — they are the specific "deliberately scoped out" seams the audit found still open across Finance, Inventory, Procurement, Sales, and Warehouse. Each is a small, bounded slice (mirroring the M8 discipline above), ordered by how much downstream value unlocks per slice:

1. **CostCenter → JournalEntryLine.** Add a nullable `CostCenterId` to `JournalEntryLine`, one EF migration, one optional param threaded through `CreateJournalEntryCommand`. This single change is what makes Budget's vs-actual query (Step below) stop being account-level-only.
2. **Budget vs-actual, cost-center-aware.** Once (1) lands, extend `GetBudgetVsActualQueryHandler`'s existing `SumPostedAmountByAccountAsync` to accept an optional `CostCenterId` filter — a one-parameter repository method change, not a new query.
3. **Tax wiring into transaction lines.** Add nullable `TaxRateId` + computed `TaxAmount` to `SalesInvoiceLine` and `PurchaseOrderLine`. Reuse the existing `ConvertAmountQuery`-style pattern from ExchangeRate: a pure calculation query (`CalculateLineTaxQuery(taxRateId, netAmount)` → `taxAmount`), not a service class. This is the highest-value single gap — every Tier-1/Tier-2 competitor in the benchmarking report has this; FusionOS currently has tax as unconnected master data.
4. **AP auto-charge from Goods Receipt.** Now that `ApLedgerEntry.RecordBillCharge` exists (M8c), wire a Kafka consumer reacting to `GoodsReceiptPosted` (the event already exists and is already fired — it is only unconsumed on the Finance side, per `ORPHANED_EVENTS_AUDIT.md`) that calls `RecordBillCharge` automatically using the PO's line totals. This retires one of the 15 documented orphaned events and removes the manual-entry-only limitation from M8c in one slice.
5. **FixedAsset depreciation posting.** Add one command, `PostMonthlyDepreciationCommand(fixedAssetId, periodEnd)`, that calls the existing (already-correct) `GetDepreciationSchedule` calculation and posts one real `JournalEntry` (Debit Depreciation Expense / Credit Accumulated Depreciation) using `AccumulatedDepreciationAccountId`. No new aggregate — this is a command handler that composes two things that already exist.
6. **Bank auto-matching (simple version only).** Not a bank-feed import (that is a separate, larger integration and stays out of scope) — just a same-amount-same-date suggestion: `SuggestMatchesForStatementLineQuery` that looks for unreconciled `JournalEntry` postings within +/-3 days and the exact amount, returned as candidates for the user to confirm via the existing `ReconcileStatementLineCommand`. Keeps the "no auto-matching algorithm" scope-out honest by making the human still confirm the match.
7. **JournalEntry balance / trial-balance report.** A read-only query (`GetTrialBalanceQuery(companyId, asOfDate)`) summing all posted `JournalEntryLine`s grouped by `AccountId` — this is the one piece of real "Finance reporting" missing, and every other Step-2 item becomes checkable once it exists.
8. **Smaller mechanical fixes**, each under an hour of real work once Phase G lets you verify them: PO over-receipt guard on `PurchaseOrderLine.AddReceivedQuantity`; RFQ resubmission (allow a supplier to replace, not just submit once); PickList real line-match validation against the originating Sales Order; wire the already-fired `PickListPacked` event into `Dispatch.Create()` (retires a second orphaned event); `InventoryLedgerEntry` cross-module WarehouseId existence check.

None of items 1–8 require a new module — they are all one-command or one-query additions inside Finance/Sales/Procurement/Warehouse, which is why they're sequenced before any new module work below.

---

## Step 3 — Phase K: test/CI confidence gate

Blocked on Phase G for the same reason everything else is — you cannot gate CI on tests that have never been run once. Once Step 1 is done:

1. Wire `dotnet test` and `npm test`/Playwright into the existing CI skeleton (already scaffolded in Phase C/D — just needs to stop being a no-op).
2. Add a coverage-threshold gate (even a low bar like 50% line coverage to start) so future slices can't silently skip tests the way none of this codebase's tests have ever silently failed to compile — you won't know which handler tests have real bugs in their NSubstitute setups until this runs once.
3. Add a basic SAST scan (e.g. a GitHub Action running `dotnet list package --vulnerable` and an npm audit) — cheap, high-value, currently entirely absent.

---

## Step 4 — Phase F: the nine scaffold-only modules, sequenced by dependency and payoff

Confirmed on disk: **Manufacturing, CRM, HRMS, Quality, Maintenance, BusinessIntelligence, Ai, Marketplace, and IntegrationHub each have exactly 5 files** (an `IModule` registration, a health controller returning `"status": "scaffolded"`, empty `ModuleMarker` classes, and an empty DbContext with no entities). **Mobile and SAP Migration have no folder at all** — not even a scaffold. Build them in this order, because each later one is more valuable with the earlier ones already in place:

1. **Manufacturing (BOM/MRP)** — builds directly on the already-solid Inventory/Warehouse foundation (weighted-average costing, batch/lot/serial, multi-UOM, full WMS are all done). A Bill of Materials is genuinely just a self-referencing `Product` structure (`BomLine: ParentProductId, ComponentProductId, Quantity`) plus a `WorkOrder` aggregate that consumes components and produces the parent product through the existing Inventory ledger — reuses `InventoryLedgerEntry` rather than inventing new stock-movement logic. Highest-leverage of the nine because it activates the FusionOS's benchmarking report's own "manufacturing depth" competitive claim.
2. **CRM** — builds on `Customer` (Sales) which already exists; a CRM slice is mostly a `Lead`/`Opportunity` pipeline feeding into an *existing* `Customer`, not a parallel customer model. Reuse `Customer.Create` as the conversion target from a won `Opportunity`.
3. **Quality** — builds on Manufacturing's `WorkOrder` (inspection points) and Procurement's `GoodsReceipt` (incoming inspection) — sequence after Manufacturing for that reason.
4. **Maintenance (EAM)** — builds on Warehouse's `Bin`/`Zone` location model for asset location, and on Finance's new `FixedAsset` (M8g) for the asset record itself — a `MaintenanceSchedule` is a thin aggregate referencing an existing `FixedAssetId`, not a new asset model.
5. **HRMS** — mostly independent of the other modules (Employee, Payroll-adjacent data), lower urgency since it doesn't unlock other modules the way Manufacturing/CRM/Quality/Maintenance chain together.
6. **BusinessIntelligence** — deliberately last among the "real feature" modules, because it's a reporting layer over everything above; building it before Manufacturing/CRM/Quality exist would mean re-touching it repeatedly as new data sources appear. Once built, it should be genuinely thin: mostly queries composing existing repository methods (the same pattern `GetTrialBalanceQuery` and `GetBudgetVsActualQuery` already establish), not a new analytics engine.
7. **Ai (AI Platform)** — per the competitive benchmarking report delivered this session, FusionOS currently has zero shipped AI capability despite nearly every competitor researched having one. The architectural bet (event backbone feeding AI in real time rather than batch ETL) only pays off once there's a real event stream across multiple real modules — sequencing this after Manufacturing/CRM/Quality/Maintenance means the AI Platform's first real feature (pick one, ship it end-to-end per the benchmarking report's own recommendation — e.g. a real-time anomaly flag on a journal-entry posting or a stock-ledger event) has actual production events to consume, not just Finance's.
8. **Marketplace** — an ecosystem play; genuinely lowest-priority until there's a stable, compiled, tested core (Phase G/K) other developers could build plugins against. Building a marketplace before Phase G is solved would be building a store for a product that doesn't run yet.
9. **IntegrationHub** — same logic as Marketplace: an integration layer is most valuable once the core is stable and running, so third-party connectors have something dependable to connect to.
10. **Mobile** — no folder exists yet. Lowest priority of all nine: it's a second frontend surface over an API that hasn't been proven to run once (Phase G). Do not start this before Phase G, or you risk building a client against endpoints that don't yet compile.
11. **SAP Migration tooling** — no folder exists yet. This is explicitly a go-to-market/sales-enablement feature (data-migration tooling for customers switching off SAP), not core product — lowest priority, revisit once there is a real deployed instance to migrate customers *into*.

Each of these nine should start exactly like Manufacturing above: one narrow, real vertical slice first (one aggregate, full CQRS+Infra+Api+tests+one frontend panel), not a wide shallow pass across the whole module — the same discipline that made M8's eight sub-slices individually trustworthy.

---

## Step 5 — Ongoing discipline (applies throughout all steps above, not a one-time task)

- Keep `docs/PROJECT_TRACKER.md` as the single source of truth — one `[x]` entry per slice, written in the same honest "written, not run" style until Phase G changes that caveat for good.
- Every slice ends with the same four checks used throughout M8: brace/paren-balance sweep, `IUnitOfWork`-namespace grep, `tsc -b --force` clean, and a tracker update using the `head -n <lastGoodLine>` + `cat >>` append method now that the tracker file is large enough that full-file rewrites risk truncation.
- Re-run the benchmarking report's own recommendation check periodically: revisit `FusionOS_Competitive_Benchmarking_Report.docx` at the start of each new module phase, since competitor AI/pricing claims in that report are dated 2025–2026 and will keep moving.
- Do not let any future module (Step 4) get ahead of Phase G/K. Building nine new scaffolds into nine new real modules without ever having compiled the six real modules first would compound, not reduce, the "written, not verified" risk this whole plan is designed to close down.

---

## One-page summary (for quick reference)

| Order | Item | Blocked on | Est. relative size |
|---|---|---|---|
| 1 | Phase G — real build, migrations, test run | Your own machine | Fixed cost, do once |
| 2 | 8 integration-gap slices inside existing modules (tax, cost-center, AP auto-charge, depreciation posting, bank matching, trial balance, + 5 mechanical fixes) | Phase G | Small each |
| 3 | Phase K — CI test/coverage/SAST gate | Phase G | Small |
| 4 | Manufacturing (BOM/MRP) | Phase G | Medium |
| 5 | CRM | Manufacturing (loosely) | Medium |
| 6 | Quality | Manufacturing | Small-medium |
| 7 | Maintenance (EAM) | FixedAsset (done), Warehouse (done) | Small-medium |
| 8 | HRMS | Phase G only | Medium |
| 9 | Business Intelligence | Manufacturing/CRM/Quality (for real data sources) | Medium |
| 10 | AI Platform | Steps 4-9 (for real event stream) | Medium, then ongoing |
| 11 | Marketplace | Phase G/K stability | Medium |
| 12 | IntegrationHub | Phase G/K stability | Medium |
| 13 | Mobile | Phase G | Large |
| 14 | SAP Migration tooling | Real deployed instance | Large, lowest priority |
