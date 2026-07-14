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
}
