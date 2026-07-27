using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Deviations.Contracts;

namespace FusionOS.Modules.Quality.Application.Deviations.Queries.GetDeviationRequestById;

public sealed record GetDeviationRequestByIdQuery(Guid CompanyId, Guid DeviationRequestId) : IQuery<DeviationRequestDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "quality.deviation.read" };
}
