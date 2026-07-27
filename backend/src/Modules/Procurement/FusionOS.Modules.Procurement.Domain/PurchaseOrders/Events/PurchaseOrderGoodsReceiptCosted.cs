using FusionOS.SharedKernel;

namespace FusionOS.Modules.Procurement.Domain.PurchaseOrders.Events;

/// <summary>
/// Raised by <see cref="PurchaseOrder.RecordGoodsReceipt"/> only when the
/// triggering Warehouse goods receipt line carried a real UnitCost — this is
/// what resolves the blocker documented on Finance's ApLedgerEntry (Phase M8c):
/// Warehouse's GoodsReceiptLineReceived event has no SupplierId, but the
/// PurchaseOrder it is received against does, so Procurement (which already
/// consumes that event to update received-status) is the natural place to
/// enrich and re-publish a costed, supplier-attributed event for Finance to
/// consume. LineAmount is QuantityReceived * UnitCost — the actual received
/// cost, not the PO's originally ordered UnitPrice, since a supplier's
/// invoice should reflect what was actually received.
/// </summary>
/// <summary>
/// TaxRateId/TaxAmount added for GST filing support (2026-07-22) — TaxAmount is
/// this receipt's prorated share of the PurchaseOrderLine's total tax (LineAmount's
/// share of the line's total), not the whole line's tax, since a receipt can be
/// partial. Both are null/0 when the line carries no TaxRateId, same convention
/// as PurchaseOrderLine itself.
/// </summary>
public sealed record PurchaseOrderGoodsReceiptCosted(
    Guid PurchaseOrderId,
    Guid CompanyId,
    Guid SupplierId,
    Guid ProductId,
    decimal QuantityReceived,
    decimal UnitCost,
    decimal LineAmount,
    Guid? TaxRateId = null,
    decimal TaxAmount = 0m) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
