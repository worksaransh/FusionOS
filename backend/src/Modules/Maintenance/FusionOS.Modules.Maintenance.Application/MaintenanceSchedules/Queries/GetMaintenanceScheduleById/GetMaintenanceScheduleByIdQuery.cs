using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Contracts;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Queries.GetMaintenanceScheduleById;

public sealed record GetMaintenanceScheduleByIdQuery(Guid CompanyId, Guid MaintenanceScheduleId) : IQuery<MaintenanceScheduleDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "maintenance.schedule.read" };
}
