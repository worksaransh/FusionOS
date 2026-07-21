using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.CompleteMaintenanceRequest;

public sealed record CompleteMaintenanceRequestCommand(Guid CompanyId, Guid MaintenanceRequestId, string? ResolutionNotes, int? ActualDowntimeMinutes = null)
    : ICommand<MaintenanceRequestDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "maintenance.request.complete" };
    public string EntityType => nameof(Domain.MaintenanceRequests.MaintenanceRequest);
    public Guid EntityId => MaintenanceRequestId;
    public string Action => "Completed";
}
