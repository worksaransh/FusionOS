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

    // Pulled into memory rather than expressed as a single GroupBy/OrderBy/First
    // EF query — that per-group "last non-null cost" shape doesn't translate
    // reliably to SQL across EF Core versions, and a company's ledger is small
    // enough for this reporting use case (same reasoning tier as the rest of
    // this repository's "recompute from history" balances).
    public async Task<IReadOnlyList<(Guid ProductId, string Sku, string Name, decimal OnHandQuantity, decimal? LastUnitCost)>> GetStockValuationAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var entries = await _context.LedgerEntries
            .Where(x => x.CompanyId == companyId)
            .Select(x => new { x.ProductId, x.QuantityDelta, x.UnitCost, x.TransactionDate })
            .ToListAsync(cancellationToken);

        if (entries.Count == 0)
            return Array.Empty<(Guid, string, string, decimal, decimal?)>();

        var products = await _context.Products
            .Where(p => p.CompanyId == companyId)
            .Select(p => new { p.Id, p.Sku, p.Name })
            .ToDictionaryAsync(p => p.Id, p => (p.Sku, p.Name), cancellationToken);

        return entries
            .GroupBy(e => e.ProductId)
            .Select(g =>
            {
                var onHand = g.Sum(e => e.QuantityDelta);
                var lastCost = g.Where(e => e.UnitCost is not null)
                    .OrderByDescending(e => e.TransactionDate)
                    .Select(e => e.UnitCost)
                    .FirstOrDefault();
                var (sku, name) = products.TryGetValue(g.Key, out var p) ? p : ("(unknown)", "(unknown product)");
                return (g.Key, sku, name, onHand, lastCost);
            })
            .ToList();
    }

    // Same "materialize then group in memory" shape as GetStockValuationAsync above,
    // but keeps each product's full ordered entry list (not just a derived summary) so
    // WeightedAverageCostCalculator can fold the real history (M9 remaining, 2026-07-16).
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
