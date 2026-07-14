using FusionOS.SharedKernel;

namespace FusionOS.Modules.Warehouse.Domain.GoodsReceipts.Events;

/// <summary>
/// Matches the "GoodsReceived.v1" event named in 03_SYSTEM_ARCHITECTURE.md §4.2's
/// event catalog — produced by Warehouse, one per received line (same granularity
/// as Inventory's StockAdjusted). Documented consumers once the outbox relay's
/// Kafka publish reaches a running consumer: Inventory (post a Stock Ledger entry
/// for the receipt), Procurement (advance the Purchase Order's received status),
/// Finance (AP match). No consumer exists yet in this codebase — see the
/// "Not built yet" note on GoodsReceipt itself.
/// </summary>
public sealed record GoodsReceiptLineReceived(
    Guid GoodsReceiptId,
    Guid CompanyId,
    Guid ProductId,
    Guid WarehouseId,
    Guid ZoneId,
    decimal QuantityReceived,
    decimal? UnitCost,
    Guid PurchaseOrderId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
