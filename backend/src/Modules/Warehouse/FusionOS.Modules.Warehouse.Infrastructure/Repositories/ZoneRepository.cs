using FusionOS.Modules.Warehouse.Application.Zones.Contracts;
using FusionOS.Modules.Warehouse.Domain.Zones;
using FusionOS.Modules.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Warehouse.Infrastructure.Repositories;

public sealed class ZoneRepository : IZoneRepository
{
    private readonly WarehouseDbContext _context;

    public ZoneRepository(WarehouseDbContext context) => _context = context;

    public Task<bool> WarehouseExistsAsync(Guid companyId, Guid warehouseId, CancellationToken cancellationToken = default) =>
        _context.Warehouses.AnyAsync(w => w.CompanyId == companyId && w.Id == warehouseId, cancellationToken);

    public Task<bool> CodeExistsAsync(Guid companyId, Guid warehouseId, string code, CancellationToken cancellationToken = default) =>
        _context.Zones.AnyAsync(z => z.CompanyId == companyId && z.WarehouseId == warehouseId && z.Code == code.Trim().ToUpper(), cancellationToken);

    public async Task AddAsync(Zone zone, CancellationToken cancellationToken = default) =>
        await _context.Zones.AddAsync(zone, cancellationToken);

    public async Task<IReadOnlyList<Zone>> ListAsync(Guid companyId, Guid warehouseId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.Zones
            .Where(z => z.CompanyId == companyId && z.WarehouseId == warehouseId)
            .OrderBy(z => z.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid warehouseId, CancellationToken cancellationToken = default) =>
        _context.Zones.CountAsync(z => z.CompanyId == companyId && z.WarehouseId == warehouseId, cancellationToken);
}
