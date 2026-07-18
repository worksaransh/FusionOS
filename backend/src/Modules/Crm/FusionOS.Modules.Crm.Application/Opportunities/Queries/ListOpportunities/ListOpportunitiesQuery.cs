using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Opportunities.Contracts;

namespace FusionOS.Modules.Crm.Application.Opportunities.Queries.ListOpportunities;

public sealed record ListOpportunitiesQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<OpportunityDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "crm.opportunity.read" };
}
