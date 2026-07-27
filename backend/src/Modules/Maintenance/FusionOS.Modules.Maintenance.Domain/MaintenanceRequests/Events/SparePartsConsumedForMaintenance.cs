using FusionOS.SharedKernel;

namespace FusionOS.Modules.Maintenance.Domain.MaintenanceRequests.Events;

/// <summary>
/// Raised when <see cref="MaintenanceRequest.RecordSparePartUsage"/> is called — the point
/// Maintenance actually consumes a spare part. Relayed via the outbox to Kafka
/// (03_SYSTEM_ARCHITECTURE.md §4.2) and consumed by Inventory's
/// SparePartsConsumedForMaintenanceConsumer, which posts the real Stock Ledger movement:
/// one negative adjustment for the consumed product in <see cref="WarehouseId"/>.
/// Maintenance never writes the Inventory ledger directly — it only announces what
/// happened and lets Inventory, which owns the ledger, apply it (same producer/consumer
/// split as Manufacturing's WorkOrderCompleted → Inventory).
///
/// Deliberately does NOT carry UnitCost — Inventory's ledger movement only needs a
/// quantity to post, same as WorkOrderCompleted's component payload only carries
/// QuantityConsumed, not cost. UnitCost stays a Maintenance-only concern (this request's
/// TotalCost calculation).
/// </summary>
public sealed record SparePartsConsumedForMaintenance(
    Guid MaintenanceRequestId,
    Guid CompanyId,
    Guid ProductId,
    Guid WarehouseId,
    decimal QuantityUsed) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
