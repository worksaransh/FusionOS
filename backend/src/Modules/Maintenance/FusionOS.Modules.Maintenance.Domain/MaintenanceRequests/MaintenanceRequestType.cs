namespace FusionOS.Modules.Maintenance.Domain.MaintenanceRequests;

/// <summary>
/// Preventive (scheduled, before a failure) vs Breakdown (reactive, after one) —
/// the two request types named in 05_MODULE_ROADMAP.md's Maintenance line item.
/// Stored as text via EF value conversion, never a native PostgreSQL enum
/// (04_DATABASE_GUIDELINES.md §10).
/// </summary>
public enum MaintenanceRequestType
{
    Preventive,
    Breakdown,
}
