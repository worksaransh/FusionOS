using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Opportunities.Contracts;

namespace FusionOS.Modules.Crm.Application.Opportunities.Commands.UpdateOpportunityForecast;

/// <summary>
/// Updates an in-flight opportunity's forecast (Probability/ExpectedCloseDate).
/// Reuses "crm.opportunity.change-stage" rather than a dedicated permission code: there is
/// no existing generic "update opportunity" permission in the catalog (only
/// create/win/lose/read/assign-account/change-stage), and updating the forecast is the same
/// kind of "modify an in-flight, still-open opportunity" action as changing its stage — not
/// closing it, not reassigning it. Adding a brand-new permission code was out of scope here
/// (PermissionCatalog.cs is owned by the orchestrating session), so change-stage was the
/// closest existing fit.
/// </summary>
public sealed record UpdateOpportunityForecastCommand(Guid CompanyId, Guid OpportunityId, decimal? Probability, DateTimeOffset? ExpectedCloseDate)
    : ICommand<OpportunityDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.opportunity.change-stage" };
    public string EntityType => nameof(Domain.Opportunities.Opportunity);
    public Guid EntityId => OpportunityId;
    public string Action => "ForecastUpdated";
}
