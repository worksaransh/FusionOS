namespace FusionOS.Modules.Warehouse.Domain.GoodsReceipts;

/// <summary>
/// Input shape for a single received line when creating a GoodsReceipt (mirrors
/// PurchaseOrderLineInput's role in Procurement). <c>BatchNumber</c>/<c>SerialNumber</c>
/// (M9 remaining — Batch/Lot/Serial tracking, 2026-07-16) are both optional free-text
/// fields, not validated against any master list — this codebase has no separate
/// "Batch"/"Serial" master-data aggregate, only a record of what was captured at
/// receipt time, same "caller supplies the data" restraint as everywhere else a
/// value isn't itself a first-class aggregate (e.g. Reason on InventoryLedgerEntry).
/// A caller would typically supply one or the other, not both — SerialNumber implies
/// per-unit tracking (so QuantityReceived would usually be 1), BatchNumber implies a
/// whole lot — but neither is enforced here; that policy decision is left to the UI.
/// </summary>
public sealed record GoodsReceiptLineInput(Guid ProductId, decimal QuantityReceived, decimal? UnitCost, string? BatchNumber = null, string? SerialNumber = null);
