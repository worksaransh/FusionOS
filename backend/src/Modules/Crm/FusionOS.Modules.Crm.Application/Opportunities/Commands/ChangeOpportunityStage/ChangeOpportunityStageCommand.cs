using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Opportunities.Contracts;
using FusionOS.Modules.Crm.Domain.Opportunities;

namespace FusionOS.Modules.Crm.Application.Opportunities.Commands.ChangeOpportunityStage;

/// <summary>
/// Moves an opportunity between the non-terminal pipeline stages (Open/Prospecting/
/// Qualification/Proposal/Negotiation). Deliberately cannot target Won/Lost — those stay
/// exclusive to WinOpportunityCommand/LoseOpportunityCommand, enforced both here (the
/// validator rejects Won/Lost as a target) and in the domain (Opportunity.ChangeStage throws).
/// </summary>
public sealed record ChangeOpportunityStageCommand(Guid CompanyId, Guid OpportunityId, OpportunityStage Stage)
    : ICommand<OpportunityDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.opportunity.change-stage" };
    public string EntityType => nameof(Domain.Opportunities.Opportunity);
    public Guid EntityId => OpportunityId;
    public string Action => "StageChanged";
}
