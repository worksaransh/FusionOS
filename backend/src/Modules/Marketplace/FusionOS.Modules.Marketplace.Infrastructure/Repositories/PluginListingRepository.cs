using FusionOS.Modules.Marketplace.Application.PluginListings.Contracts;
using FusionOS.Modules.Marketplace.Domain.PluginListings;
using FusionOS.Modules.Marketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Marketplace.Infrastructure.Repositories;

public sealed class PluginListingRepository : IPluginListingRepository
{
    private readonly MarketplaceDbContext _context;

    public PluginListingRepository(MarketplaceDbContext context) => _context = context;

    public Task<PluginListing?> GetByIdAsync(Guid companyId, Guid pluginListingId, CancellationToken cancellationToken = default) =>
        _context.PluginListings.FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Id == pluginListingId, cancellationToken);

    public Task<bool> ExistsAsync(Guid companyId, Guid pluginListingId, CancellationToken cancellationToken = default) =>
        _context.PluginListings.AnyAsync(p => p.CompanyId == companyId && p.Id == pluginListingId, cancellationToken);

    public async Task AddAsync(PluginListing listing, CancellationToken cancellationToken = default) =>
        await _context.PluginListings.AddAsync(listing, cancellationToken);

    public async Task<IReadOnlyList<PluginListing>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderBy(p => p.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    private IQueryable<PluginListing> Filtered(Guid companyId, string? search)
    {
        var query = _context.PluginListings.Where(p => p.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.Code, pattern) || EF.Functions.ILike(p.Name, pattern));
        }
        return query;
    }
}
