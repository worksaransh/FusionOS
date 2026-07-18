using FusionOS.Modules.Warehouse.Application.PickLists.Contracts;
using FusionOS.Modules.Warehouse.Domain.PickLists;
using FusionOS.Modules.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Warehouse.Infrastructure.Repositories;

public sealed class PickListRepository : IPickListRepository
{
    private readonly WarehouseDbContext _context;

    public PickListRepository(WarehouseDbContext context) => _context = context;

    // .Include(x => x.Lines) is required any time a parent whose child collection is backed by a
    // private field is queried — without it, Lines comes back empty (same note as GoodsReceiptRepository).
    public Task<PickList?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.PickLists.Include(p => p.Lines).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task AddAsync(PickList pickList, CancellationToken cancellationToken = default) =>
        await _context.PickLists.AddAsync(pickList, cancellationToken);

    public async Task<IReadOnlyList<PickList>> ListAsync(Guid companyId, Guid warehouseId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.PickLists
            .Include(p => p.Lines)
            .Where(p => p.CompanyId == companyId && p.WarehouseId == warehouseId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid warehouseId, CancellationToken cancellationToken = default) =>
        _context.PickLists.CountAsync(p => p.CompanyId == companyId && p.WarehouseId == warehouseId, cancellationToken);
}
