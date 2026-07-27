using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Campaigns.Contracts;

namespace FusionOS.Modules.Crm.Application.Campaigns.Queries.GetCampaignById;

public sealed record GetCampaignByIdQuery(Guid CompanyId, Guid CampaignId) : IQuery<CampaignWithMembersDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "crm.campaign.read" };
}
