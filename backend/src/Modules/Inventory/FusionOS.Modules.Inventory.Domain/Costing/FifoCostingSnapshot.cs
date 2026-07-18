namespace FusionOS.Modules.Inventory.Domain.Costing;

/// <summary>
/// Result of folding a product's <see cref="Ledger.InventoryLedgerEntry"/> history through
/// <see cref="FifoCostCalculator"/>. Deliberately not persisted anywhere — same
/// "valuation is derived, never stored" rule as <see cref="InventoryCostingSnapshot"/>
/// (04_DATABASE_GUIDELINES.md §12). CurrentUnitCost is the weighted average of whatever
/// layers remain on hand after FIFO consumption — a single number for display purposes,
/// not a claim that FIFO tracks one uniform cost (it doesn't; see FifoCostCalculator).
/// </summary>
public sealed record FifoCostingSnapshot(
    decimal OnHandQuantity,
    decimal CurrentUnitCost,
    decimal TotalValuation,
    decimal CumulativeCostOfGoodsSold);
