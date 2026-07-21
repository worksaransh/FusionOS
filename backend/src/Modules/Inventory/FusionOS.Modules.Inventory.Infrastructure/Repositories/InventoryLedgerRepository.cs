using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Domain.Ledger;
using FusionOS.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Inventory.Infrastructure.Repositories;

public sealed class InventoryLedgerRepository : IInventoryLedgerRepository
{
    private readonly InventoryDbContext _context;

    public InventoryLedgerRepository(InventoryDbContext context) => _context = context;

    public async Task AddAsync(InventoryLedgerEntry entry, CancellationToken cancellationToken = default) =>
        await _context.LedgerEntries.AddAsync(entry, cancellationToken);

    public Task<decimal> SumQuantityAsync(Guid companyId, Guid productId, Guid? warehouseId, CancellationToken cancellationToken = default)
    {
        var query = _context.LedgerEntries.Where(x => x.CompanyId == companyId && x.ProductId == productId);
        if (warehouseId is not null)
            query = query.Where(x => x.WarehouseId == warehouseId);

        return query.SumAsync(x => x.QuantityDelta, cancellationToken);
    }

    public async Task<IReadOnlyList<InventoryLedgerEntry>> ListAsync(Guid companyId, Guid productId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.LedgerEntries
            .Where(x => x.CompanyId == companyId && x.ProductId == productId)
            .OrderByDescending(x => x.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid productId, CancellationToken cancellationToken = default) =>
        _context.LedgerEntries.CountAsync(x => x.CompanyId == companyId && x.ProductId == productId, cancellationToken);

    // OnHandQuantity is now a SQL-side GroupBy/Sum (same translated-aggregation
    // shape as JournalEntryRepository's line sums) instead of materializing every
    // receipt/dispatch/adjustment/transfer/cycle-count entry the company has ever
    // posted just to add them up in memory — the query now returns one summed row
    // per product, not the whole ledger.
    //
    // LastUnitCost ("most recent non-null UnitCost per product, by TransactionDate")
    // is a per-group ordered-pick-first shape, which — as documented here before this
    // fix — does not reliably translate to SQL via a plain GroupBy/OrderBy/First
    // projection in EF Core. It still needs an in-memory pass, but that pass now only
    // pulls the cost-bearing rows (UnitCost is not null — typically just the receipt
    // side of the ledger) and only three columns each, instead of the entire ledger's
    // full entity rows including the (usually far more numerous) dispatches that never
    // carry a cost.
    public async Task<IReadOnlyList<(Guid ProductId, string Sku, string Name, decimal OnHandQuantity, decimal? LastUnitCost)>> GetStockValuationAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var quantities = await _context.LedgerEntries
            .Where(x => x.CompanyId == companyId)
            .GroupBy(x => x.ProductId)
            .Select(g => new { ProductId = g.Key, OnHandQuantity = g.Sum(x => x.QuantityDelta) })
            .ToListAsync(cancellationToken);

        if (quantities.Count == 0)
            return Array.Empty<(Guid, string, string, decimal, decimal?)>();

        var costRows = await _context.LedgerEntries
            .Where(x => x.CompanyId == companyId && x.UnitCost != null)
            .Select(x => new { x.ProductId, x.UnitCost, x.TransactionDate })
            .ToListAsync(cancellationToken);

        var lastCostByProduct = costRows
            .GroupBy(x => x.ProductId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.TransactionDate).Select(x => x.UnitCost).First());

        var products = await _context.Products
            .Where(p => p.CompanyId == companyId)
            .Select(p => new { p.Id, p.Sku, p.Name })
            .ToDictionaryAsync(p => p.Id, p => (p.Sku, p.Name), cancellationToken);

        return quantities
            .Select(q =>
            {
                var (sku, name) = products.TryGetValue(q.ProductId, out var p) ? p : ("(unknown)", "(unknown product)");
                var lastCost = lastCostByProduct.TryGetValue(q.ProductId, out var cost) ? cost : (decimal?)null;
                return (q.ProductId, sku, name, q.OnHandQuantity, lastCost);
            })
            .ToList();
    }

    // Unlike GetStockValuationAsync above (now a SQL-side sum) and GetPriceHistoryAsync
    // on PurchaseOrderRepository (a caller-supplied ProductId pushed into the WHERE
    // clause), this method has no per-product scope to push down: its one caller,
    // GetInventoryValuationReportQueryHandler, needs every product's full entry
    // history in the same call, because WeightedAverageCostCalculator/FifoCostCalculator
    // fold that ordered history with running-balance/queue logic (average cost blending,
    // FIFO layer consumption) that cannot be expressed as a SQL aggregate — both
    // calculators' own doc comments are explicit that they are pure, non-querying
    // functions and expect the full unfiltered history handed to them. So the full
    // per-company ledger scan here is inherent to what a whole-company valuation
    // report requires, not a needless load of more than what's needed — it was left
    // as-is rather than forcing a fake bounded rewrite. A genuine fix (e.g. periodic
    // costing checkpoints/snapshots so each report run only folds the delta since the
    // last checkpoint) would be an architectural change beyond a repository query
    // rewrite, and is out of scope here.
    public async Task<IReadOnlyList<(Guid ProductId, string Sku, string Name, IReadOnlyList<InventoryLedgerEntry> Entries)>> GetLedgerEntriesByProductAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var entries = await _context.LedgerEntries
            .Where(x => x.CompanyId == companyId)
            .ToListAsync(cancellationToken);

        if (entries.Count == 0)
            return Array.Empty<(Guid, string, string, IReadOnlyList<InventoryLedgerEntry>)>();

        var products = await _context.Products
            .Where(p => p.CompanyId == companyId)
            .Select(p => new { p.Id, p.Sku, p.Name })
            .ToDictionaryAsync(p => p.Id, p => (p.Sku, p.Name), cancellationToken);

        return entries
            .GroupBy(e => e.ProductId)
            .Select(g =>
            {
                var (sku, name) = products.TryGetValue(g.Key, out var p) ? p : ("(unknown)", "(unknown product)");
                return (g.Key, sku, name, (IReadOnlyList<InventoryLedgerEntry>)g.ToList());
            })
            .ToList();
    }
}
