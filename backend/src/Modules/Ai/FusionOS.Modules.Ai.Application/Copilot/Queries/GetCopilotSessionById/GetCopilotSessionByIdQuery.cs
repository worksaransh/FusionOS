using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Ai.Application.Copilot.Contracts;

namespace FusionOS.Modules.Ai.Application.Copilot.Queries.GetCopilotSessionById;

public sealed record GetCopilotSessionByIdQuery(Guid CompanyId, Guid SessionId) : IQuery<CopilotSessionDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "ai.copilot.read" };
}
