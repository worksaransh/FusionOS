using FusionOS.SharedKernel;

namespace FusionOS.Modules.Manufacturing.Domain.WorkOrders.Events;

/// <summary>One component consumed by a completed work order, carried on the integration event.</summary>
public sealed record WorkOrderComponentConsumption(Guid ComponentProductId, decimal QuantityConsumed);

/// <summary>
/// Raised when a work order is completed — the point manufacturing actually moves stock.
/// Relayed via the outbox to Kafka (03_SYSTEM_ARCHITECTURE.md §4.2) and consumed by
/// Inventory's WorkOrderCompletedConsumer, which posts the real Stock Ledger movements:
/// one negative adjustment per consumed component and one positive adjustment for the
/// produced parent product, all in <see cref="WarehouseId"/>. Manufacturing never writes
/// the Inventory ledger directly — it only announces what happened and lets Inventory,
/// which owns the ledger, apply it (same producer/consumer split as
/// GoodsReceiptLineReceived → Inventory).
/// </summary>
public sealed record WorkOrderCompleted(
    Guid WorkOrderId,
    Guid CompanyId,
    Guid WarehouseId,
    Guid ProductId,
    decimal QuantityProduced,
    IReadOnlyList<WorkOrderComponentConsumption> Components) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
