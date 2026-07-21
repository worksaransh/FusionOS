using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.CorrectiveActions.Contracts;

namespace FusionOS.Modules.Quality.Application.CorrectiveActions.Commands.CreateCorrectiveAction;

public sealed record CreateCorrectiveActionCommand(
    Guid CompanyId,
    Guid NonConformanceReportId,
    string RootCauseDescription,
    string CorrectiveActionDescription,
    string PreventiveActionDescription,
    Guid AssignedToUserId,
    DateTimeOffset DueDate)
    : ICommand<CorrectiveActionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.corrective-action.create" };
    public string EntityType => nameof(Domain.CorrectiveActions.CorrectiveAction);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
