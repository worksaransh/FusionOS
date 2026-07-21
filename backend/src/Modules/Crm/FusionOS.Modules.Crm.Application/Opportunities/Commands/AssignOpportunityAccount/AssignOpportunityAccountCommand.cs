using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Opportunities.Contracts;

namespace FusionOS.Modules.Crm.Application.Opportunities.Commands.AssignOpportunityAccount;

/// <summary>Sets or clears (pass null) which Account this opportunity's organization corresponds to — same shape as Sales' AssignPriceListCommand.</summary>
public sealed record AssignOpportunityAccountCommand(Guid CompanyId, Guid OpportunityId, Guid? AccountId)
    : ICommand<OpportunityDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.opportunity.assign-account" };
    public string EntityType => nameof(Domain.Opportunities.Opportunity);
    public Guid EntityId => OpportunityId;
    public string Action => "AccountAssigned";
}
