using FusionOS.Modules.Inventory.Domain.Ledger;

namespace FusionOS.Modules.Inventory.Domain.Costing;

/// <summary>
/// Pure domain service (M9 remaining — Inventory costing, 2026-07-16). Folds an ordered
/// <see cref="InventoryLedgerEntry"/> history for a single product+warehouse into a running
/// weighted-average unit cost, matching the standard weighted-average-cost (WAC) method:
///
/// - On every stock-in entry (QuantityDelta &gt; 0), the incoming cost (its UnitCost, or the
///   current running average if UnitCost is null — e.g. a dispatch reversal or manual
///   adjustment recorded with no cost) is blended into the running average, weighted by
///   quantity: NewAverage = (OldQty*OldAverage + InQty*InCost) / (OldQty + InQty).
/// - On every stock-out entry (QuantityDelta &lt; 0), the running average is used as the cost
///   of goods sold for the issued quantity; the average itself is unchanged by an issue (WAC's
///   defining property vs. FIFO/LIFO) and the cumulative COGS total accumulates.
/// - Entries are folded in <see cref="InventoryLedgerEntry.TransactionDate"/> order — callers
///   are responsible for passing a full, unfiltered history for the product+warehouse pair;
///   this calculator does no querying or filtering itself (pure function, no I/O), so it lives
///   in Domain rather than Application.
///
/// This deliberately does not persist a mutable "current average cost" field on Product or any
/// other aggregate — see 04_DATABASE_GUIDELINES.md §12, "valuation is derived, never stored as
/// the sole source of truth." Every call recomputes from the ledger; callers needing this
/// repeatedly for reporting should cache at the query/repository layer, not in the domain model.
/// </summary>
public static class WeightedAverageCostCalculator
{
    public static InventoryCostingSnapshot Calculate(IReadOnlyList<InventoryLedgerEntry> entries)
    {
        var quantity = 0m;
        var averageCost = 0m;
        var cumulativeCogs = 0m;

        foreach (var entry in entries.OrderBy(e => e.TransactionDate))
        {
            if (entry.QuantityDelta > 0m)
            {
                var incomingCost = entry.UnitCost ?? averageCost;
                var newQuantity = quantity + entry.QuantityDelta;
                averageCost = newQuantity == 0m
                    ? 0m
                    : ((quantity * averageCost) + (entry.QuantityDelta * incomingCost)) / newQuantity;
                quantity = newQuantity;
            }
            else if (entry.QuantityDelta < 0m)
            {
                var issuedQuantity = -entry.QuantityDelta;
                cumulativeCogs += issuedQuantity * averageCost;
                quantity += entry.QuantityDelta;
            }
        }

        return new InventoryCostingSnapshot(quantity, averageCost, quantity * averageCost, cumulativeCogs);
    }
}
