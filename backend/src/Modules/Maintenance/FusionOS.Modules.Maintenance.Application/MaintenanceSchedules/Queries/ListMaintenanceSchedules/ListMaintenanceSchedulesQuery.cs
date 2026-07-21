using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Contracts;
using FusionOS.Modules.Maintenance.Domain.MaintenanceSchedules;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Queries.ListMaintenanceSchedules;

/// <summary>
/// AssetId is optional — omitted, this lists every schedule for the company; supplied, it
/// scopes to one Asset's preventive-maintenance plan. DueFilter is also optional: omitted
/// returns every schedule regardless of due date; DueSoon/Overdue narrow to the two views
/// called out in the roadmap line item for preventive maintenance scheduling (see
/// MaintenanceScheduleRepository for the "due soon" window definition).
/// </summary>
public sealed record ListMaintenanceSchedulesQuery(Guid CompanyId, Guid? AssetId = null, MaintenanceScheduleDueFilter? DueFilter = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<MaintenanceScheduleDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "maintenance.schedule.read" };
}
