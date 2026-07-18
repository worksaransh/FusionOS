using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Opportunities.Contracts;

namespace FusionOS.Modules.Crm.Application.Opportunities.Queries.GetOpportunityById;

public sealed record GetOpportunityByIdQuery(Guid CompanyId, Guid OpportunityId) : IQuery<OpportunityDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "crm.opportunity.read" };
}
