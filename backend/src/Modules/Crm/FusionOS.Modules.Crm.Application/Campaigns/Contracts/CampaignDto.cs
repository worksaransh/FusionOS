namespace FusionOS.Modules.Crm.Application.Campaigns.Contracts;

public sealed record CampaignDto(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset StartDate,
    DateTimeOffset? EndDate,
    bool IsActive);

/// <summary>Single place that turns a Campaign aggregate into its DTO, shared by every handler that returns one.</summary>
public static class CampaignMapper
{
    public static CampaignDto ToDto(Domain.Campaigns.Campaign campaign) => new(
        campaign.Id,
        campaign.Name,
        campaign.Description,
        campaign.StartDate,
        campaign.EndDate,
        campaign.IsActive);
}
