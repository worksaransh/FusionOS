using FusionOS.Modules.Crm.Application.Campaigns.Contracts;
using FusionOS.Modules.Crm.Domain.Campaigns;
using FusionOS.Modules.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Crm.Infrastructure.Repositories;

public sealed class CampaignRepository : ICampaignRepository
{
    private readonly CrmDbContext _context;

    public CampaignRepository(CrmDbContext context) => _context = context;

    public Task<Campaign?> GetByIdAsync(Guid companyId, Guid campaignId, CancellationToken cancellationToken = default) =>
        _context.Set<Campaign>().FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Id == campaignId, cancellationToken);

    public Task<bool> ExistsAsync(Guid companyId, Guid campaignId, CancellationToken cancellationToken = default) =>
        _context.Set<Campaign>().AnyAsync(c => c.CompanyId == companyId && c.Id == campaignId, cancellationToken);

    public async Task AddAsync(Campaign campaign, CancellationToken cancellationToken = default) =>
        await _context.Set<Campaign>().AddAsync(campaign, cancellationToken);

    public async Task<IReadOnlyList<Campaign>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    private IQueryable<Campaign> Filtered(Guid companyId, string? search)
    {
        var query = _context.Set<Campaign>().Where(c => c.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(c => EF.Functions.ILike(c.Name, pattern));
        }

        return query;
    }
}
