using FusionOS.BuildingBlocks.Application.Abstractions;

namespace FusionOS.Modules.Manufacturing.Application.WorkCenters.Queries.GetWorkCenterCapacityLoad;

/// <summary>
/// Capacity Planning (Phase 6, 2026-07-22): available vs. required hours for one work
/// center over [PeriodStart, PeriodEnd] (both inclusive).
/// </summary>
public sealed record GetWorkCenterCapacityLoadQuery(Guid CompanyId, Guid WorkCenterId, DateTimeOffset PeriodStart, DateTimeOffset PeriodEnd)
    : IQuery<WorkCenterCapacityLoadDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "manufacturing.capacity.read" };
}

/// <summary>
/// AvailableHours = WorkCenter.CapacityHoursPerDay × the number of days in the period
/// (inclusive). RequiredHours sums StandardMinutes/60 across every scheduled operation
/// (on a Released work order) whose ScheduledStart falls within the period AND whose
/// free-text WorkCenter name matches this work center's Code/Name (case-insensitive) —
/// see the handler's doc comment for why this is a best-effort string match, not a hard FK.
/// </summary>
public sealed record WorkCenterCapacityLoadDto(Guid WorkCenterId, decimal AvailableHours, decimal RequiredHours, decimal UtilizationPercentage);
