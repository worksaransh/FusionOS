namespace FusionOS.Modules.Quality.Domain.Inspections;

/// <summary>
/// What an inspection is checking. IncomingGoods inspects a Procurement/Warehouse Goods
/// Receipt; Production inspects a Manufacturing Work Order's output; Final inspects a
/// Sales Dispatch prior to shipment — for all three, <see cref="Inspection.ReferenceId"/>
/// is the same opaque cross-module id (a GRN id, a WorkOrder id, or a Dispatch id
/// respectively), never existence-validated here. Stored as text via EF value conversion,
/// never a native PostgreSQL enum — 04_DATABASE_GUIDELINES.md §10.
/// </summary>
public enum InspectionType
{
    IncomingGoods,
    Production,
    Final,
}
