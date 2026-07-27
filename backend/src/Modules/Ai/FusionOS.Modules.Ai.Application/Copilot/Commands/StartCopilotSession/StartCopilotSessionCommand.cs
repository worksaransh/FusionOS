using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Ai.Application.Copilot.Contracts;

namespace FusionOS.Modules.Ai.Application.Copilot.Commands.StartCopilotSession;

public sealed record StartCopilotSessionCommand(Guid CompanyId, Guid UserId, string? Title)
    : ICommand<CopilotSessionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "ai.copilot.chat" };
    public string EntityType => nameof(Domain.Copilot.CopilotSession);
    public Guid EntityId { get; init; }
    public string Action => "Started";
}
