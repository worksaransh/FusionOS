using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Leads.Contracts;

namespace FusionOS.Modules.Crm.Application.Leads.Commands.RecalculateLeadScore;

/// <summary>
/// Manually re-runs LeadScoringService against a lead's current AccountId and logged
/// Activities (a user triggers this after, say, logging a call — the score doesn't
/// recompute itself automatically on every related change, same "explicit recalculation"
/// shape as Finance's recompute-style commands).
///
/// Permission: "crm.lead.recalculate-score", a dedicated code added once the imperfect
/// "crm.lead.qualify" stopgap this command originally reused was flagged for review.
/// </summary>
public sealed record RecalculateLeadScoreCommand(Guid CompanyId, Guid LeadId)
    : ICommand<LeadDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.lead.recalculate-score" };
    public string EntityType => nameof(Domain.Leads.Lead);
    public Guid EntityId => LeadId;
    public string Action => "ScoreRecalculated";
}
