using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Contracts;
using FusionOS.Modules.Maintenance.Domain.MaintenanceSchedules;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Commands.UpdateMaintenanceSchedule;

/// <summary>Update deliberately excludes AssetId — same "the reference is set at creation" convention as UpdateCostCenterCommand excluding Code.</summary>
public sealed record UpdateMaintenanceScheduleCommand(Guid CompanyId, Guid MaintenanceScheduleId, MaintenanceScheduleFrequency Frequency, string Description, DateTimeOffset NextDueDate)
    : ICommand<MaintenanceScheduleDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "maintenance.schedule.update" };
    public string EntityType => nameof(Domain.MaintenanceSchedules.MaintenanceSchedule);
    public Guid EntityId => MaintenanceScheduleId;
    public string Action => "Updated";
}
