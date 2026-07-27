namespace FusionOS.Modules.Crm.Application.Campaigns.Contracts;

public interface ICampaignRepository
{
    Task<Domain.Campaigns.Campaign?> GetByIdAsync(Guid companyId, Guid campaignId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid companyId, Guid campaignId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Campaigns.Campaign campaign, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Campaigns.Campaign>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
