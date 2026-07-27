using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Opportunities.Contracts;

namespace FusionOS.Modules.Crm.Application.Opportunities.Commands.AssignOpportunityOwner;

/// <summary>
/// Sets or clears (pass null) which Core user owns/works this opportunity — "territory
/// via ownership". Same manual-assignment-only scope as AssignLeadOwnerCommand (see its
/// doc comment) — no territory rule engine here. OwnerId is never existence-validated
/// against Core's User table, same opaque cross-module reference convention as
/// MaintenanceRequest.AssignTechnician.
/// </summary>
public sealed record AssignOpportunityOwnerCommand(Guid CompanyId, Guid OpportunityId, Guid? OwnerId)
    : ICommand<OpportunityDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.opportunity.assign-owner" };
    public string EntityType => nameof(Domain.Opportunities.Opportunity);
    public Guid EntityId => OpportunityId;
    public string Action => "OwnerAssigned";
}
