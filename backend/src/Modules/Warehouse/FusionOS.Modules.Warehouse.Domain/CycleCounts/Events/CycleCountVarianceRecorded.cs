using FusionOS.SharedKernel;

namespace FusionOS.Modules.Warehouse.Domain.CycleCounts.Events;

/// <summary>
/// Cross-module event, same "GoodsReceiptLineReceived" style relay (outbox ->
/// Kafka -> consumer, 03_SYSTEM_ARCHITECTURE.md §4.2). Raised only when a
/// completed count actually found a variance (a balanced count needs no
/// downstream adjustment). Inventory's consumer turns this into an
/// InventoryLedgerEntry.RecordAdjustment call, the same factory the manual
/// "Adjust Stock" feature and the GoodsReceipt consumer both use — this is
/// deliberately not a new ledger concept, just a new producer of adjustments.
/// </summary>
public sealed record CycleCountVarianceRecorded(
    Guid CycleCountId,
    Guid CompanyId,
    Guid ProductId,
    Guid WarehouseId,
    decimal VarianceQuantity) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
