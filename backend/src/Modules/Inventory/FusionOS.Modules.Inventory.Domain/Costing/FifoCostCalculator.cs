using FusionOS.Modules.Inventory.Domain.Ledger;

namespace FusionOS.Modules.Inventory.Domain.Costing;

/// <summary>
/// Pure domain service (Phase 1 closeout, 2026-07-18 — 05_MODULE_ROADMAP.md's Inventory
/// "Inventory Valuation (FIFO, Weighted Average Cost)" line item; WAC was built M9, this
/// closes the FIFO half). Folds an ordered <see cref="InventoryLedgerEntry"/> history for a
/// single product+warehouse into a first-in-first-out cost basis, maintaining a queue of
/// "layers" (one per stock-in entry, each its own quantity + unit cost):
///
/// - On every stock-in entry (QuantityDelta &gt; 0), a new layer is enqueued at the back with
///   its own UnitCost (0 when null — e.g. a dispatch reversal or manual adjustment recorded
///   with no cost; unlike WAC there is no running average to fall back on, since FIFO's whole
///   point is that each layer keeps its own distinct cost).
/// - On every stock-out entry (QuantityDelta &lt; 0), the issued quantity is consumed from the
///   FRONT of the queue first (oldest layer), accumulating COGS at each consumed layer's own
///   unit cost — partially consuming a layer if the issued quantity doesn't exactly match it,
///   and moving on to the next-oldest layer if a single layer isn't enough to cover the issue.
///   An issue larger than everything on hand (should not happen against a well-formed ledger,
///   but the ledger never enforces non-negative stock — same restraint as
///   WeightedAverageCostCalculator) simply drains every remaining layer at whatever cost they
///   carry and stops; it does not throw.
/// - Entries are folded in <see cref="InventoryLedgerEntry.TransactionDate"/> order — same
///   "callers pass a full, unfiltered history, this calculator does no querying itself" contract
///   as WeightedAverageCostCalculator (pure function, no I/O, lives in Domain).
///
/// CurrentUnitCost on the returned snapshot is the weighted average of whatever layers remain
/// after all consumption — a single display number, not a claim that FIFO stock is uniformly
/// costed (see FifoCostingSnapshot's own doc comment).
/// </summary>
public static class FifoCostCalculator
{
    public static FifoCostingSnapshot Calculate(IReadOnlyList<InventoryLedgerEntry> entries)
    {
        var layers = new Queue<(decimal Quantity, decimal UnitCost)>();
        var cumulativeCogs = 0m;

        foreach (var entry in entries.OrderBy(e => e.TransactionDate))
        {
            if (entry.QuantityDelta > 0m)
            {
                layers.Enqueue((entry.QuantityDelta, entry.UnitCost ?? 0m));
            }
            else if (entry.QuantityDelta < 0m)
            {
                var remainingToIssue = -entry.QuantityDelta;
                while (remainingToIssue > 0m && layers.Count > 0)
                {
                    var layer = layers.Dequeue();
                    var consumed = Math.Min(layer.Quantity, remainingToIssue);
                    cumulativeCogs += consumed * layer.UnitCost;
                    remainingToIssue -= consumed;

                    var layerRemaining = layer.Quantity - consumed;
                    if (layerRemaining > 0m)
                    {
                        // Put the partially-consumed layer back at the front so the next
                        // issue still draws from it before any newer layer.
                        var requeued = new List<(decimal Quantity, decimal UnitCost)> { (layerRemaining, layer.UnitCost) };
                        requeued.AddRange(layers);
                        layers = new Queue<(decimal Quantity, decimal UnitCost)>(requeued);
                    }
                }
            }
        }

        var onHandQuantity = layers.Sum(l => l.Quantity);
        var totalValuation = layers.Sum(l => l.Quantity * l.UnitCost);
        var currentUnitCost = onHandQuantity == 0m ? 0m : totalValuation / onHandQuantity;

        return new FifoCostingSnapshot(onHandQuantity, currentUnitCost, totalValuation, cumulativeCogs);
    }
}
