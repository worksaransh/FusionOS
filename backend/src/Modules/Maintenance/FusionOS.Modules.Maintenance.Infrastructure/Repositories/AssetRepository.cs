using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using FusionOS.Modules.Maintenance.Domain.Assets;
using FusionOS.Modules.Maintenance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Maintenance.Infrastructure.Repositories;

public sealed class AssetRepository : IAssetRepository
{
    private readonly MaintenanceDbContext _context;

    public AssetRepository(MaintenanceDbContext context) => _context = context;

    public Task<Asset?> GetByIdAsync(Guid companyId, Guid assetId, CancellationToken cancellationToken = default) =>
        _context.Assets.FirstOrDefaultAsync(a => a.CompanyId == companyId && a.Id == assetId, cancellationToken);

    public Task<bool> ExistsAsync(Guid companyId, Guid assetId, CancellationToken cancellationToken = default) =>
        _context.Assets.AnyAsync(a => a.CompanyId == companyId && a.Id == assetId, cancellationToken);

    public async Task AddAsync(Asset asset, CancellationToken cancellationToken = default) =>
        await _context.Assets.AddAsync(asset, cancellationToken);

    public async Task<IReadOnlyList<Asset>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderBy(a => a.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    private IQueryable<Asset> Filtered(Guid companyId, string? search)
    {
        var query = _context.Assets.Where(a => a.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(a => EF.Functions.ILike(a.Code, pattern) || EF.Functions.ILike(a.Name, pattern));
        }
        return query;
    }
}
