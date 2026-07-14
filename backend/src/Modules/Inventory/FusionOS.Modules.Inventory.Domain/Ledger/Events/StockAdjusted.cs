using FusionOS.SharedKernel;

namespace FusionOS.Modules.Inventory.Domain.Ledger.Events;

/// <summary>
/// Cross-module integration event candidate (03_SYSTEM_ARCHITECTURE.md §4.2,
/// event catalog: InventoryAdjusted.v1) — Finance (valuation) and BI/AI would
/// subscribe to this once the outbox relay is fully wired for this module.
/// </summary>
public sealed record StockAdjusted(Guid LedgerEntryId, Guid CompanyId, Guid ProductId, Guid WarehouseId, decimal QuantityDelta) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
