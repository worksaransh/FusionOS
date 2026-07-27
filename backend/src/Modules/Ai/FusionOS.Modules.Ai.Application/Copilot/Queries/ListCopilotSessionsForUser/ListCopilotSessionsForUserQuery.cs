using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Ai.Application.Copilot.Contracts;

namespace FusionOS.Modules.Ai.Application.Copilot.Queries.ListCopilotSessionsForUser;

public sealed record ListCopilotSessionsForUserQuery(Guid CompanyId, Guid UserId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<CopilotSessionDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "ai.copilot.read" };
}
