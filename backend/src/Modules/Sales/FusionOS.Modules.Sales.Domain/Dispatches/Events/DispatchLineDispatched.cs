using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.Dispatches.Events;

/// <summary>
/// Candidate cross-module integration event (03_SYSTEM_ARCHITECTURE.md §4.2),
/// symmetric to Warehouse's GoodsReceiptLineReceived but for the outbound side.
/// Intended consumers once wired up: Inventory (stock-out ledger entry —
/// symmetric to GoodsReceipt crediting stock in), Warehouse (physical pick/pack/
/// ship execution tracking). No consumer exists yet, same documented gap as
/// every other integration-event candidate in this codebase.
/// </summary>
public sealed record DispatchLineDispatched(
    Guid DispatchId,
    Guid CompanyId,
    Guid SalesOrderId,
    Guid ProductId,
    Guid WarehouseId,
    decimal QuantityDispatched) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
