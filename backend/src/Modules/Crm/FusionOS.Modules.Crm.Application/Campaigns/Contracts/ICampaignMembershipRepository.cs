namespace FusionOS.Modules.Crm.Application.Campaigns.Contracts;

public interface ICampaignMembershipRepository
{
    Task<Domain.Campaigns.CampaignMembership?> GetByIdAsync(Guid companyId, Guid membershipId, CancellationToken cancellationToken = default);
    Task<Domain.Campaigns.CampaignMembership?> FindAsync(Guid companyId, Guid campaignId, string entityType, Guid entityId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Campaigns.CampaignMembership membership, CancellationToken cancellationToken = default);
    void Remove(Domain.Campaigns.CampaignMembership membership);
    Task<IReadOnlyList<Domain.Campaigns.CampaignMembership>> ListByCampaignAsync(Guid companyId, Guid campaignId, CancellationToken cancellationToken = default);
}
