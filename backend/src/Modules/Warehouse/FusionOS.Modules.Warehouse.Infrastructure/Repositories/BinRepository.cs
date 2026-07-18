using FusionOS.Modules.Warehouse.Application.Bins.Contracts;
using FusionOS.Modules.Warehouse.Domain.Bins;
using FusionOS.Modules.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Warehouse.Infrastructure.Repositories;

public sealed class BinRepository : IBinRepository
{
    private readonly WarehouseDbContext _context;

    public BinRepository(WarehouseDbContext context) => _context = context;

    public Task<Bin?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Bins.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<bool> ZoneExistsAsync(Guid companyId, Guid zoneId, CancellationToken cancellationToken = default) =>
        _context.Zones.AnyAsync(z => z.CompanyId == companyId && z.Id == zoneId, cancellationToken);

    public Task<bool> ExistsAsync(Guid companyId, Guid binId, CancellationToken cancellationToken = default) =>
        _context.Bins.AnyAsync(b => b.CompanyId == companyId && b.Id == binId, cancellationToken);

    public Task<Bin?> GetFirstActiveBinAsync(Guid companyId, Guid zoneId, CancellationToken cancellationToken = default) =>
        _context.Bins
            .Where(b => b.CompanyId == companyId && b.ZoneId == zoneId && b.IsActive)
            .OrderBy(b => b.Code)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> CodeExistsAsync(Guid companyId, Guid zoneId, string code, CancellationToken cancellationToken = default) =>
        _context.Bins.AnyAsync(b => b.CompanyId == companyId && b.ZoneId == zoneId && b.Code == code.Trim().ToUpper(), cancellationToken);

    public async Task AddAsync(Bin bin, CancellationToken cancellationToken = default) =>
        await _context.Bins.AddAsync(bin, cancellationToken);

    public async Task<IReadOnlyList<Bin>> ListAsync(Guid companyId, Guid zoneId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.Bins
            .Where(b => b.CompanyId == companyId && b.ZoneId == zoneId)
            .OrderBy(b => b.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid zoneId, CancellationToken cancellationToken = default) =>
        _context.Bins.CountAsync(b => b.CompanyId == companyId && b.ZoneId == zoneId, cancellationToken);
}
