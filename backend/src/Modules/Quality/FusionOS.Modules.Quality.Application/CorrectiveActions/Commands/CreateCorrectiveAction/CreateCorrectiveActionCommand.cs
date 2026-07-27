using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.CorrectiveActions.Contracts;
using FusionOS.Modules.Quality.Domain.CorrectiveActions;

namespace FusionOS.Modules.Quality.Application.CorrectiveActions.Commands.CreateCorrectiveAction;

public sealed record CreateCorrectiveActionCommand(
    Guid CompanyId,
    Guid NonConformanceReportId,
    string RootCauseDescription,
    string CorrectiveActionDescription,
    string PreventiveActionDescription,
    Guid AssignedToUserId,
    DateTimeOffset DueDate,
    RcaMethod RcaMethod = RcaMethod.Other,
    string? Why1 = null,
    string? Why2 = null,
    string? Why3 = null,
    string? Why4 = null,
    string? Why5 = null)
    : ICommand<CorrectiveActionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.corrective-action.create" };
    public string EntityType => nameof(Domain.CorrectiveActions.CorrectiveAction);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
