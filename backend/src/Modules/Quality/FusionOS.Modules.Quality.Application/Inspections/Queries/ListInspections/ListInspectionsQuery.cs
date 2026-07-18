using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;

namespace FusionOS.Modules.Quality.Application.Inspections.Queries.ListInspections;

public sealed record ListInspectionsQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<InspectionDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "quality.inspection.read" };
}
