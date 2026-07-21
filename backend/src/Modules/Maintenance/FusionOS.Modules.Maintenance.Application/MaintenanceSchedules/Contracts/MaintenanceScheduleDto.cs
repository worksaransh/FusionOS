namespace FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Contracts;

public sealed record MaintenanceScheduleDto(
    Guid Id,
    Guid AssetId,
    string Description,
    string Frequency,
    DateTimeOffset NextDueDate,
    bool IsActive,
    DateTimeOffset CreatedAt);

/// <summary>Single place that turns a MaintenanceSchedule aggregate into its DTO, shared by every handler that returns one.</summary>
public static class MaintenanceScheduleMapper
{
    public static MaintenanceScheduleDto ToDto(Domain.MaintenanceSchedules.MaintenanceSchedule schedule) => new(
        schedule.Id,
        schedule.AssetId,
        schedule.Description,
        schedule.Frequency.ToString(),
        schedule.NextDueDate,
        schedule.IsActive,
        schedule.CreatedAt);
}
