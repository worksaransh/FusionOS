namespace FusionOS.Modules.Maintenance.Domain.MaintenanceSchedules;

/// <summary>Optional due-date filter for listing schedules — backs the "due soon"/"overdue" views called out in the roadmap line item for preventive maintenance scheduling.</summary>
public enum MaintenanceScheduleDueFilter
{
    DueSoon,
    Overdue,
}
