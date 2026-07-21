using FusionOS.Modules.Warehouse.Application.Racks.Contracts;
using FusionOS.Modules.Warehouse.Domain.Racks;
using FusionOS.Modules.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Warehouse.Infrastructure.Repositories;

public sealed class RackRepository : IRackRepository
{
    private readonly WarehouseDbContext _context;

    public RackRepository(WarehouseDbContext context) => _context = context;

    public Task<Rack?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Racks.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<bool> ZoneExistsAsync(Guid companyId, Guid zoneId, CancellationToken cancellationToken = default) =>
        _context.Zones.AnyAsync(z => z.CompanyId == companyId && z.Id == zoneId, cancellationToken);

    public Task<bool> CodeExistsAsync(Guid companyId, Guid zoneId, string code, CancellationToken cancellationToken = default) =>
        _context.Racks.AnyAsync(r => r.CompanyId == companyId && r.ZoneId == zoneId && r.Code == code.Trim().ToUpper(), cancellationToken);

    public async Task AddAsync(Rack rack, CancellationToken cancellationToken = default) =>
        await _context.Racks.AddAsync(rack, cancellationToken);

    public async Task<IReadOnlyList<Rack>> ListAsync(Guid companyId, Guid zoneId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.Racks
            .Where(r => r.CompanyId == companyId && r.ZoneId == zoneId)
            .OrderBy(r => r.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid zoneId, CancellationToken cancellationToken = default) =>
        _context.Racks.CountAsync(r => r.CompanyId == companyId && r.ZoneId == zoneId, cancellationToken);
}
