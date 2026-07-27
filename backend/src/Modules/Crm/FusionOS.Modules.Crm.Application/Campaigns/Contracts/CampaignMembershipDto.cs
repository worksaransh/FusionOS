namespace FusionOS.Modules.Crm.Application.Campaigns.Contracts;

public sealed record CampaignMembershipDto(
    Guid Id,
    Guid CampaignId,
    string EntityType,
    Guid EntityId,
    DateTimeOffset AddedAt);

/// <summary>Single place that turns a CampaignMembership aggregate into its DTO, shared by every handler that returns one.</summary>
public static class CampaignMembershipMapper
{
    public static CampaignMembershipDto ToDto(Domain.Campaigns.CampaignMembership membership) => new(
        membership.Id,
        membership.CampaignId,
        membership.EntityType,
        membership.EntityId,
        membership.AddedAt);
}

/// <summary>A Campaign plus its current member list — the shape GetCampaignById returns (list-only queries return CampaignDto alone).</summary>
public sealed record CampaignWithMembersDto(CampaignDto Campaign, IReadOnlyList<CampaignMembershipDto> Members);
