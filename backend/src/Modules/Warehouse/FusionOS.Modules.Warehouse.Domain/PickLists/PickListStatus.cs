namespace FusionOS.Modules.Warehouse.Domain.PickLists;

/// <summary>Stored as text via EF value conversion — 04_DATABASE_GUIDELINES.md §10 (same convention as SalesOrderStatus/CycleCountStatus).</summary>
public enum PickListStatus
{
    Pending,
    Assigned,
    Picked,
    Packed,
}
