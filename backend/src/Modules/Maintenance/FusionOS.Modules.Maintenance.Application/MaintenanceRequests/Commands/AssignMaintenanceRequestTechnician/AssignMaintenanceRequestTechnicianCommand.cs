using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.AssignMaintenanceRequestTechnician;

public sealed record AssignMaintenanceRequestTechnicianCommand(Guid CompanyId, Guid MaintenanceRequestId, Guid TechnicianUserId)
    : ICommand<MaintenanceRequestDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "maintenance.request.assign-technician" };
    public string EntityType => nameof(Domain.MaintenanceRequests.MaintenanceRequest);
    public Guid EntityId => MaintenanceRequestId;
    public string Action => "TechnicianAssigned";
}
