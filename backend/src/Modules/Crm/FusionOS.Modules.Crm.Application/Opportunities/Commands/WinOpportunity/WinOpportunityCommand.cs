using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Opportunities.Contracts;

namespace FusionOS.Modules.Crm.Application.Opportunities.Commands.WinOpportunity;

public sealed record WinOpportunityCommand(Guid CompanyId, Guid OpportunityId, string CustomerCode)
    : ICommand<OpportunityDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.opportunity.win" };
    public string EntityType => nameof(Domain.Opportunities.Opportunity);
    public Guid EntityId => OpportunityId;
    public string Action => "Won";
}
