using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Campaigns.Contracts;

namespace FusionOS.Modules.Crm.Application.Campaigns.Queries.ListCampaigns;

public sealed record ListCampaignsQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<CampaignDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "crm.campaign.read" };
}
