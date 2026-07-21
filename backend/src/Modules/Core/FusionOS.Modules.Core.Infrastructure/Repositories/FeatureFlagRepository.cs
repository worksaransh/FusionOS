using FusionOS.Modules.Core.Application.FeatureFlags.Contracts;
using FusionOS.Modules.Core.Domain.FeatureFlags;
using FusionOS.Modules.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Core.Infrastructure.Repositories;

/// <summary>
/// Uses _context.Set&lt;FeatureFlag&gt;() rather than a CoreDbContext.FeatureFlags DbSet
/// property — CoreDbContext.cs is being touched by another parallel change and wasn't
/// edited here (see this feature's report for the exact DbSet line still needed there).
/// EF still tracks/queries FeatureFlag correctly via Set&lt;T&gt;() as long as
/// FeatureFlagConfiguration (in this same assembly) is picked up by CoreDbContext's
/// existing modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreDbContext).Assembly)
/// call, which it is.
/// </summary>
public sealed class FeatureFlagRepository : IFeatureFlagRepository
{
    private readonly CoreDbContext _context;

    public FeatureFlagRepository(CoreDbContext context) => _context = context;

    private DbSet<FeatureFlag> FeatureFlags => _context.Set<FeatureFlag>();

    public Task<bool> KeyExistsAsync(Guid companyId, string key, CancellationToken cancellationToken = default) =>
        FeatureFlags.AnyAsync(f => f.CompanyId == companyId && f.Key == key.Trim(), cancellationToken);

    public Task<FeatureFlag?> GetByIdAsync(Guid companyId, Guid featureFlagId, CancellationToken cancellationToken = default) =>
        FeatureFlags.FirstOrDefaultAsync(f => f.CompanyId == companyId && f.Id == featureFlagId, cancellationToken);

    public Task<FeatureFlag?> GetByKeyAsync(Guid companyId, string key, CancellationToken cancellationToken = default) =>
        FeatureFlags.FirstOrDefaultAsync(f => f.CompanyId == companyId && f.Key == key.Trim(), cancellationToken);

    public async Task AddAsync(FeatureFlag featureFlag, CancellationToken cancellationToken = default) =>
        await FeatureFlags.AddAsync(featureFlag, cancellationToken);

    public async Task<IReadOnlyList<FeatureFlag>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderBy(f => f.Key)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    private IQueryable<FeatureFlag> Filtered(Guid companyId, string? search)
    {
        var query = FeatureFlags.Where(f => f.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(f => EF.Functions.ILike(f.Key, pattern) || EF.Functions.ILike(f.Name, pattern));
        }
        return query;
    }
}
