using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Ai.Application.Copilot.Contracts;

namespace FusionOS.Modules.Ai.Application.Copilot.Commands.SendCopilotMessage;

public sealed record SendCopilotMessageCommand(Guid CompanyId, Guid SessionId, string Message)
    : ICommand<CopilotSessionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "ai.copilot.chat" };
    public string EntityType => nameof(Domain.Copilot.CopilotSession);
    public Guid EntityId => SessionId;
    public string Action => "MessageSent";
}
