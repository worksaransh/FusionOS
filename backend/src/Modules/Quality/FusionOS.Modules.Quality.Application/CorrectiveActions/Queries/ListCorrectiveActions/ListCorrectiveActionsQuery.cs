using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.CorrectiveActions.Contracts;

namespace FusionOS.Modules.Quality.Application.CorrectiveActions.Queries.ListCorrectiveActions;

/// <summary>NonConformanceReportId is optional — omitted, this lists every CAPA plan for the company; supplied, it scopes to the plans raised against one NCR.</summary>
public sealed record ListCorrectiveActionsQuery(Guid CompanyId, Guid? NonConformanceReportId = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<CorrectiveActionDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "quality.corrective-action.read" };
}
