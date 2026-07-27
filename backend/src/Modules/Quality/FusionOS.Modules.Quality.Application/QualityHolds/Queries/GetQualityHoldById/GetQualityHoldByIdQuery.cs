using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.QualityHolds.Contracts;

namespace FusionOS.Modules.Quality.Application.QualityHolds.Queries.GetQualityHoldById;

public sealed record GetQualityHoldByIdQuery(Guid CompanyId, Guid QualityHoldId) : IQuery<QualityHoldDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "quality.quality-hold.read" };
}
