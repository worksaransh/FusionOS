using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.CompleteMaintenanceRequestChecklistItem;

/// <summary>
/// Reuses maintenance.request.start rather than a dedicated checklist-item permission — no
/// separate permission was reserved for ticking off an individual checklist step, and ticking
/// one off is part of doing the work while a request is InProgress/Open, the same phase gated by
/// maintenance.request.start, not the terminal maintenance.request.complete transition (which
/// resolves the whole request, not one of its steps). Documented here since this is a judgment
/// call, not a pre-reserved permission.
/// </summary>
public sealed record CompleteMaintenanceRequestChecklistItemCommand(Guid CompanyId, Guid MaintenanceRequestId, Guid ChecklistItemId)
    : ICommand<MaintenanceRequestDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "maintenance.request.start" };
    public string EntityType => nameof(Domain.MaintenanceRequests.MaintenanceRequest);
    public Guid EntityId => MaintenanceRequestId;
    public string Action => "ChecklistItemCompleted";
}
