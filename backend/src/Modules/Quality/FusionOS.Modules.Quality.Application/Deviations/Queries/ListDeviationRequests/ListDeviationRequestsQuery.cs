using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Deviations.Contracts;

namespace FusionOS.Modules.Quality.Application.Deviations.Queries.ListDeviationRequests;

/// <summary>InspectionId is optional — omitted, this lists every deviation request for the company; supplied, it scopes to the requests raised against one inspection.</summary>
public sealed record ListDeviationRequestsQuery(Guid CompanyId, Guid? InspectionId = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<DeviationRequestDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "quality.deviation.read" };
}
