using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Contracts;
using FusionOS.Modules.Warehouse.Domain.GoodsReceipts;
using FusionOS.Modules.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Warehouse.Infrastructure.Repositories;

public sealed class GoodsReceiptRepository : IGoodsReceiptRepository
{
    private readonly WarehouseDbContext _context;

    public GoodsReceiptRepository(WarehouseDbContext context) => _context = context;

    // .Include(x => x.Lines) is required any time a parent whose child collection is backed by a
    // private field is queried — without it, Lines comes back empty (same note as PickListRepository).
    public Task<GoodsReceipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.GoodsReceipts.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<bool> ZoneExistsAsync(Guid companyId, Guid warehouseId, Guid zoneId, CancellationToken cancellationToken = default) =>
        _context.Zones.AnyAsync(z => z.CompanyId == companyId && z.WarehouseId == warehouseId && z.Id == zoneId, cancellationToken);

    public async Task AddAsync(GoodsReceipt receipt, CancellationToken cancellationToken = default) =>
        await _context.GoodsReceipts.AddAsync(receipt, cancellationToken);

    public async Task<IReadOnlyList<GoodsReceipt>> ListAsync(Guid companyId, Guid warehouseId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.GoodsReceipts
            .Include(r => r.Lines)
            .Where(r => r.CompanyId == companyId && r.WarehouseId == warehouseId)
            .OrderByDescending(r => r.ReceivedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid warehouseId, CancellationToken cancellationToken = default) =>
        _context.GoodsReceipts.CountAsync(r => r.CompanyId == companyId && r.WarehouseId == warehouseId, cancellationToken);
}
