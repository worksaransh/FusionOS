namespace FusionOS.Modules.Inventory.Application.Ledger.Contracts;

public interface IInventoryLedgerRepository
{
    Task AddAsync(Domain.Ledger.InventoryLedgerEntry entry, CancellationToken cancellationToken = default);

    Task<decimal> SumQuantityAsync(Guid companyId, Guid productId, Guid? warehouseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Domain.Ledger.InventoryLedgerEntry>> ListAsync(Guid companyId, Guid productId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountAsync(Guid companyId, Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// One row per product that has ever had a ledger entry — OnHandQuantity is
    /// the running sum of every QuantityDelta (same "recompute from history"
    /// rule as SumQuantityAsync), LastUnitCost is the most recent non-null
    /// UnitCost by TransactionDate (Phase M6, 2026-07-15 stock valuation
    /// report). Sku/Name come from this same module's own Product table — no
    /// cross-module join needed since Product lives in Inventory too.
    /// </summary>
    Task<IReadOnlyList<(Guid ProductId, string Sku, string Name, decimal OnHandQuantity, decimal? LastUnitCost)>> GetStockValuationAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// One entry per product that has ever had a ledger entry, each carrying that
    /// product's *full* ordered entry history across all warehouses (M9 remaining —
    /// Inventory costing, 2026-07-16) — callers fold each product's Entries through
    /// WeightedAverageCostCalculator to get a real weighted-average valuation, rather
    /// than this repository trying to express the fold as SQL (same "recompute from
    /// history in memory" reasoning as GetStockValuationAsync above).
    /// </summary>
    Task<IReadOnlyList<(Guid ProductId, string Sku, string Name, IReadOnlyList<Domain.Ledger.InventoryLedgerEntry> Entries)>> GetLedgerEntriesByProductAsync(Guid companyId, CancellationToken cancellationToken = default);
}
