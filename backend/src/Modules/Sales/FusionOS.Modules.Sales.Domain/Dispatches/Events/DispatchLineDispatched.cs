using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.Dispatches.Events;

/// <summary>
/// Cross-module integration event (03_SYSTEM_ARCHITECTURE.md §4.2), symmetric
/// to Warehouse's GoodsReceiptLineReceived but for the outbound side. Now has
/// three real consumers: Inventory's DispatchLineDispatchedConsumer (stock-out
/// ledger entry, symmetric to GoodsReceipt crediting stock in), Quality's
/// DispatchLineDispatchedConsumer (auto-opens an outbound Inspection), and
/// Ai's DispatchLineDispatchedConsumer (demand-history signal for
/// GetDemandForecastQuery). No longer the "no consumer yet" candidate this
/// doc comment originally described.
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
