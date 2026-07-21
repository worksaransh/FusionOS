using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.CorrectiveActions.Contracts;

namespace FusionOS.Modules.Quality.Application.CorrectiveActions.Queries.GetCorrectiveActionById;

public sealed record GetCorrectiveActionByIdQuery(Guid CompanyId, Guid CorrectiveActionId) : IQuery<CorrectiveActionDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "quality.corrective-action.read" };
}
