using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using FusionOS.Modules.Finance.Domain.FixedAssets;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class FixedAssetRepository : IFixedAssetRepository
{
    private readonly FinanceDbContext _context;

    public FixedAssetRepository(FinanceDbContext context) => _context = context;

    public Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default) =>
        _context.FixedAssets.AnyAsync(a => a.CompanyId == companyId && a.Code == code.Trim().ToUpper(), cancellationToken);

    public Task<FixedAsset?> GetByIdAsync(Guid companyId, Guid fixedAssetId, CancellationToken cancellationToken = default) =>
        _context.FixedAssets.FirstOrDefaultAsync(a => a.CompanyId == companyId && a.Id == fixedAssetId, cancellationToken);

    public async Task AddAsync(FixedAsset fixedAsset, CancellationToken cancellationToken = default) =>
        await _context.FixedAssets.AddAsync(fixedAsset, cancellationToken);

    public async Task<IReadOnlyList<FixedAsset>> ListAsync(Guid companyId, bool? isDisposed, bool? isActive, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, isDisposed, isActive)
            .OrderBy(a => a.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, bool? isDisposed, bool? isActive, CancellationToken cancellationToken = default) =>
        Filtered(companyId, isDisposed, isActive).CountAsync(cancellationToken);

    private IQueryable<FixedAsset> Filtered(Guid companyId, bool? isDisposed, bool? isActive)
    {
        var query = _context.FixedAssets.Where(a => a.CompanyId == companyId);
        if (isDisposed.HasValue)
            query = query.Where(a => a.IsDisposed == isDisposed.Value);
        if (isActive.HasValue)
            query = query.Where(a => a.IsActive == isActive.Value);
        return query;
    }
}
