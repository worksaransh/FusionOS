using FusionOS.Modules.Warehouse.Application.CycleCounts.Contracts;
using FusionOS.Modules.Warehouse.Domain.CycleCounts;
using FusionOS.Modules.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Warehouse.Infrastructure.Repositories;

public sealed class CycleCountRepository : ICycleCountRepository
{
    private readonly WarehouseDbContext _context;

    public CycleCountRepository(WarehouseDbContext context) => _context = context;

    public Task<CycleCount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.CycleCounts.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> BinExistsAsync(Guid companyId, Guid zoneId, Guid binId, CancellationToken cancellationToken = default) =>
        _context.Bins.AnyAsync(b => b.CompanyId == companyId && b.ZoneId == zoneId && b.Id == binId, cancellationToken);

    public async Task AddAsync(CycleCount cycleCount, CancellationToken cancellationToken = default) =>
        await _context.CycleCounts.AddAsync(cycleCount, cancellationToken);

    public async Task<IReadOnlyList<CycleCount>> ListAsync(Guid companyId, Guid warehouseId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.CycleCounts
            .Where(c => c.CompanyId == companyId && c.WarehouseId == warehouseId)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid warehouseId, CancellationToken cancellationToken = default) =>
        _context.CycleCounts.CountAsync(c => c.CompanyId == companyId && c.WarehouseId == warehouseId, cancellationToken);
}
