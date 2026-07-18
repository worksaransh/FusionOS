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
public sealed record PurchaseOrderGoodsReceiptCosted(
    Guid PurchaseOrderId,
    Guid CompanyId,
    Guid SupplierId,
    Guid ProductId,
    decimal QuantityReceived,
    decimal UnitCost,
    decimal LineAmount) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
