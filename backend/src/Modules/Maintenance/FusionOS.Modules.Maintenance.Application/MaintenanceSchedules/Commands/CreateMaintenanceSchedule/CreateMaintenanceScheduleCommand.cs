using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Contracts;
using FusionOS.Modules.Maintenance.Domain.MaintenanceSchedules;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Commands.CreateMaintenanceSchedule;

public sealed record CreateMaintenanceScheduleCommand(Guid CompanyId, Guid AssetId, MaintenanceScheduleFrequency Frequency, string Description, DateTimeOffset NextDueDate)
    : ICommand<MaintenanceScheduleDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "maintenance.schedule.create" };
    public string EntityType => nameof(Domain.MaintenanceSchedules.MaintenanceSchedule);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
