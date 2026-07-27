using FusionOS.Modules.Crm.Application.Campaigns.Contracts;
using FusionOS.Modules.Crm.Domain.Campaigns;
using FusionOS.Modules.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Crm.Infrastructure.Repositories;

public sealed class CampaignMembershipRepository : ICampaignMembershipRepository
{
    private readonly CrmDbContext _context;

    public CampaignMembershipRepository(CrmDbContext context) => _context = context;

    public Task<CampaignMembership?> GetByIdAsync(Guid companyId, Guid membershipId, CancellationToken cancellationToken = default) =>
        _context.Set<CampaignMembership>().FirstOrDefaultAsync(m => m.CompanyId == companyId && m.Id == membershipId, cancellationToken);

    public Task<CampaignMembership?> FindAsync(Guid companyId, Guid campaignId, string entityType, Guid entityId, CancellationToken cancellationToken = default) =>
        _context.Set<CampaignMembership>().FirstOrDefaultAsync(
            m => m.CompanyId == companyId && m.CampaignId == campaignId && m.EntityType == entityType && m.EntityId == entityId,
            cancellationToken);

    public async Task AddAsync(CampaignMembership membership, CancellationToken cancellationToken = default) =>
        await _context.Set<CampaignMembership>().AddAsync(membership, cancellationToken);

    public void Remove(CampaignMembership membership) => _context.Set<CampaignMembership>().Remove(membership);

    public async Task<IReadOnlyList<CampaignMembership>> ListByCampaignAsync(Guid companyId, Guid campaignId, CancellationToken cancellationToken = default) =>
        await _context.Set<CampaignMembership>()
            .Where(m => m.CompanyId == companyId && m.CampaignId == campaignId)
            .OrderByDescending(m => m.AddedAt)
            .ToListAsync(cancellationToken);
}
