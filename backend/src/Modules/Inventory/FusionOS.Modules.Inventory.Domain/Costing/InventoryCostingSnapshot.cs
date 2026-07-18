namespace FusionOS.Modules.Inventory.Domain.Costing;

/// <summary>
/// Result of folding a product+warehouse's <see cref="Ledger.InventoryLedgerEntry"/> history
/// through <see cref="WeightedAverageCostCalculator"/>. Deliberately not persisted anywhere —
/// per 04_DATABASE_GUIDELINES.md §12, valuation is always derived from the append-only ledger,
/// never stored as the sole source of truth.
/// </summary>
public sealed record InventoryCostingSnapshot(
    decimal OnHandQuantity,
    decimal WeightedAverageUnitCost,
    decimal TotalValuation,
    decimal CumulativeCostOfGoodsSold);
