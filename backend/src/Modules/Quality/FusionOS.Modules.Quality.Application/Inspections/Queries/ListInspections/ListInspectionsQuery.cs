using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using FusionOS.Modules.Quality.Domain.Inspections;

namespace FusionOS.Modules.Quality.Application.Inspections.Queries.ListInspections;

/// <summary>Type is optional — omitted, this lists every inspection for the company; supplied (e.g. Final), it scopes to just that InspectionType.</summary>
public sealed record ListInspectionsQuery(Guid CompanyId, InspectionType? Type = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<InspectionDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "quality.inspection.read" };
}
