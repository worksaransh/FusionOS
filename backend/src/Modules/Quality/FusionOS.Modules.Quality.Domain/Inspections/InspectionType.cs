namespace FusionOS.Modules.Quality.Domain.Inspections;

/// <summary>
/// What an inspection is checking. IncomingGoods inspects a Procurement/Warehouse Goods
/// Receipt; Production inspects a Manufacturing Work Order's output. Stored as text via EF
/// value conversion, never a native PostgreSQL enum — 04_DATABASE_GUIDELINES.md §10.
/// </summary>
public enum InspectionType
{
    IncomingGoods,
    Production,
}
