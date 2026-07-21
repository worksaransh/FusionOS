using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.CorrectiveActions.Contracts;

namespace FusionOS.Modules.Quality.Application.CorrectiveActions.Commands.CloseCorrectiveAction;

public sealed record CloseCorrectiveActionCommand(Guid CompanyId, Guid CorrectiveActionId)
    : ICommand<CorrectiveActionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.corrective-action.close" };
    public string EntityType => nameof(Domain.CorrectiveActions.CorrectiveAction);
    public Guid EntityId => CorrectiveActionId;
    public string Action => "Closed";
}
