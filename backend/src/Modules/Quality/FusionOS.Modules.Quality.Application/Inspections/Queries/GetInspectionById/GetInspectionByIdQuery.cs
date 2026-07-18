using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;

namespace FusionOS.Modules.Quality.Application.Inspections.Queries.GetInspectionById;

public sealed record GetInspectionByIdQuery(Guid CompanyId, Guid InspectionId) : IQuery<InspectionDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "quality.inspection.read" };
}
