using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.StartMaintenanceRequest;

public sealed record StartMaintenanceRequestCommand(Guid CompanyId, Guid MaintenanceRequestId)
    : ICommand<MaintenanceRequestDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "maintenance.request.start" };
    public string EntityType => nameof(Domain.MaintenanceRequests.MaintenanceRequest);
    public Guid EntityId => MaintenanceRequestId;
    public string Action => "Started";
}
