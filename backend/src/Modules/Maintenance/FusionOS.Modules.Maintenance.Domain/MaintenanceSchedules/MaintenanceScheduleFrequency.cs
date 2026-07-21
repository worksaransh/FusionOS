namespace FusionOS.Modules.Maintenance.Domain.MaintenanceSchedules;

/// <summary>
/// How often a preventive maintenance schedule recurs. An enum, not a free-form
/// interval-in-days field or a cron expression — this codebase prefers enums over
/// cron-like fields for recurrence (there is no existing recurring-schedule
/// concept elsewhere to match; this is the first one). Stored as text via EF
/// value conversion, never a native PostgreSQL enum, same as
/// MaintenanceRequestType (04_DATABASE_GUIDELINES.md §10).
/// </summary>
public enum MaintenanceScheduleFrequency
{
    Weekly,
    Monthly,
    Quarterly,
    Annual,
}
