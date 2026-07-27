using FusionOS.SharedKernel;

namespace FusionOS.Modules.Warehouse.Domain.GoodsReceipts.Events;

/// <summary>
/// Matches the "GoodsReceived.v1" event named in 03_SYSTEM_ARCHITECTURE.md §4.2's
/// event catalog — produced by Warehouse, one per received line (same granularity
/// as Inventory's StockAdjusted). Now has three real consumers: Inventory's
/// GoodsReceiptLineReceivedConsumer (posts a Stock Ledger entry for the receipt),
/// Procurement's GoodsReceiptLineReceivedConsumer (advances the Purchase Order's
/// received status), and Quality's GoodsReceiptLineReceivedConsumer (auto-opens
/// an inbound Inspection). Finance reacts one step further downstream instead —
/// its InvoiceIssuedConsumer/PurchaseOrderGoodsReceiptCostedConsumer key off
/// Sales'/Procurement's own costed events, not this one directly.
/// </summary>
public sealed record GoodsReceiptLineReceived(
    Guid GoodsReceiptId,
    Guid CompanyId,
    Guid ProductId,
    Guid WarehouseId,
    Guid ZoneId,
    decimal QuantityReceived,
    decimal? UnitCost,
    Guid PurchaseOrderId,
    string? BatchNumber = null,
    string? SerialNumber = null) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
