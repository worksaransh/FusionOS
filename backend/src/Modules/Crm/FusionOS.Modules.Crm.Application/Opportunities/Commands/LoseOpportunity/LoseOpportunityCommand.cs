using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Opportunities.Contracts;

namespace FusionOS.Modules.Crm.Application.Opportunities.Commands.LoseOpportunity;

public sealed record LoseOpportunityCommand(Guid CompanyId, Guid OpportunityId)
    : ICommand<OpportunityDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.opportunity.lose" };
    public string EntityType => nameof(Domain.Opportunities.Opportunity);
    public Guid EntityId => OpportunityId;
    public string Action => "Lost";
}
