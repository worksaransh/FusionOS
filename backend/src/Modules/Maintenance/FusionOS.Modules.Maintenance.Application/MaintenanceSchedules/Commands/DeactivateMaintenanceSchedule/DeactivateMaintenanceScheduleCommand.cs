using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Contracts;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Commands.DeactivateMaintenanceSchedule;

public sealed record DeactivateMaintenanceScheduleCommand(Guid CompanyId, Guid MaintenanceScheduleId)
    : ICommand<MaintenanceScheduleDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "maintenance.schedule.deactivate" };
    public string EntityType => nameof(Domain.MaintenanceSchedules.MaintenanceSchedule);
    public Guid EntityId => MaintenanceScheduleId;
    public string Action => "Deactivated";
}
