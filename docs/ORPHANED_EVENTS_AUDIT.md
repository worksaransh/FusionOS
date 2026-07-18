# FusionOS — Orphaned Domain Events Audit

**Date:** 2026-07-15 (Phase M4)

## 1. What this is

Every domain event raised anywhere in FusionOS via an aggregate's `Raise()` call is
automatically staged as an outbox row and published to Kafka by
`BaseDbContext.StageOutboxMessages()` (see `backend/src/Shared/FusionOS.BuildingBlocks.Infrastructure/Persistence/BaseDbContext.cs`)
— there is no distinction in this codebase between an "internal-only" event and a
genuine cross-module integration event. That means every event raised anywhere is,
in practice, a real Kafka message some other module could consume.

This document is the result of grepping every `Raise(new X(...))` call against every
existing Kafka consumer class, to answer: which of these published events actually
have something listening, and — for the ones that don't — is that a gap or is it fine?

This corrects an earlier, rougher estimate in `docs/PROJECT_TRACKER.md`/
`docs/REMEDIATION_ROADMAP.md` of "~11 orphaned events." A full grep-based count
found **15**, not 11. The higher number changes nothing about the plan — it just
means the earlier figure was an estimate and this one is a verified count.

## 2. Method

- Grepped every `Raise(new ...)` call across all six real modules (Core, Inventory,
  Warehouse, Procurement, Sales, Finance) to enumerate every distinct event type ever
  published.
- Grepped every class implementing the Kafka consumer interface to enumerate every
  event type that actually has a listener.
- Diffed the two lists.

**Result:** 18 distinct event types are raised. Only 4 consumer classes exist,
covering 3 of those 18 event types:

| Event | Consumer(s) |
|---|---|
| `GoodsReceiptLineReceived` | Inventory's `GoodsReceiptLineReceivedConsumer` (stock-on-hand) and Procurement's `GoodsReceiptLineReceivedConsumer` (PO fulfillment tracking) |
| `DispatchLineDispatched` | Inventory's `DispatchLineDispatchedConsumer` (stock-on-hand) |
| `InvoiceIssued` | Finance's `InvoiceIssuedConsumer` (AR ledger charge) |

That leaves the 15 events below with zero consumers.

## 3. The 15 orphaned events

For each, this is a judgment call on whether it's a real gap (an obviously missing
side effect that should be wired up) or a non-issue (nothing in the system needs to
react to it yet). Per the engagement's standing rule, no consumer is invented just to
give one of these an owner — a consumer that does nothing meaningful is worse than no
consumer at all.

- **`CompanyCreated`** (Core) — no other module provisions per-tenant state in
  reaction to a new company; every module's data is created lazily, on the user's own
  action (create the first Warehouse, the first Product, etc.), not pre-seeded off
  this event. Not a gap.
- **`ProductCreated`** (Inventory) — Warehouse/Procurement/Sales all reference
  products by id directly through their own repositories; nothing needs to react to
  creation itself. Not a gap.
- **`WarehouseCreated`** / **`ZoneCreated`** (Warehouse) — same reasoning; nothing
  elsewhere reacts to a warehouse or zone existing. Not a gap.
- **`SupplierCreated`** (Procurement) — same reasoning. Not a gap.
- **`CustomerCreated`** (Sales) — same reasoning today. This is the one entry on this
  list that a future CRM module (Phase F, explicitly parked) would plausibly consume
  — but CRM doesn't exist yet, so there's nothing to wire.
- **`PurchaseOrderCreated`** (Procurement) — a PO is a Draft at this point; nothing
  reacts until a Goods Receipt is manually recorded against it (a separate, direct
  user action, not event-driven). Not a gap.
- **`PurchaseOrderApproved`** (Procurement) — the obvious real-world side effect is
  "notify the supplier," but the Notification module (Phase M7) has no working
  delivery channel yet (blocked on a provider decision — SendGrid/Twilio/SES). Wiring
  a consumer now would mean writing to a `Notification` entity nothing ever delivers.
  Left unwired on purpose; revisit once Phase M7's delivery channel exists.
- **`SalesOrderCreated`** (Sales) — Draft state, same reasoning as `PurchaseOrderCreated`.
  Not a gap.
- **`SalesOrderConfirmed`** (Sales) — the obvious real-world side effect is reserving
  stock against the order, but Inventory has no "reserved" quantity concept today —
  only actual stock-on-hand. Adding stock reservation is a real feature (would belong
  in Phase M9/M10's WMS-depth work), not a five-line consumer. Left unwired on
  purpose.
- **`InvoiceCreated`** (Sales) — Draft state; Finance intentionally only reacts to
  `InvoiceIssued` (the actual posting event), not to draft creation. This is correct
  existing behavior, not an oversight.
- **`JournalEntryCreated`** (Finance) — Draft state, same reasoning. Not a gap.
- **`JournalEntryPosted`** (Finance) — the obvious future consumer is a Reports/
  Dashboard module reacting to ledger postings, but Phase M6 (Reports + Dashboard)
  doesn't exist yet. Left unwired on purpose.
- **`AccountCreated`** (Finance) — no other module reacts to a new GL account
  existing. Not a gap.
- **`StockAdjusted`** (Inventory) — the obvious future consumer is a low-stock alert
  via Notifications, blocked on the same Phase M7 delivery-channel decision as
  `PurchaseOrderApproved` above. Left unwired on purpose.

## 4. Summary

Of the 15, 10 are simply not needed by anything in the system as it exists today
(`CompanyCreated`, `ProductCreated`, `WarehouseCreated`, `ZoneCreated`,
`SupplierCreated`, `PurchaseOrderCreated`, `SalesOrderCreated`, `InvoiceCreated`,
`JournalEntryCreated`, `AccountCreated` — reference-data or Draft-state events with
no downstream consumer anywhere in the architecture). The other 5
(`CustomerCreated`, `PurchaseOrderApproved`, `SalesOrderConfirmed`,
`JournalEntryPosted`, `StockAdjusted`) have a plausible real consumer, but that
consumer lives in a module (CRM, Notifications, WMS reservation depth, Reports) that
doesn't exist yet. None of the 15 represent a bug or a dropped side effect in the
current system — they're correctly-scoped non-issues or explicitly-deferred work,
not silent gaps.
