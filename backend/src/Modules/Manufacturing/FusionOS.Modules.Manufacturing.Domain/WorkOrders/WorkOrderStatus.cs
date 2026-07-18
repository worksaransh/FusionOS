namespace FusionOS.Modules.Manufacturing.Domain.WorkOrders;

/// <summary>Stored as text via EF value conversion, never a native PostgreSQL enum — 04_DATABASE_GUIDELINES.md §10.</summary>
public enum WorkOrderStatus
{
    Draft,
    Released,
    Completed,
    Cancelled,
}
