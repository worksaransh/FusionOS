namespace FusionOS.Modules.Maintenance.Domain.MaintenanceRequests;

/// <summary>
/// One spare part consumed against a <see cref="MaintenanceRequest"/> while work is
/// InProgress — the follow-up <see cref="MaintenanceRequest"/>'s own doc comment flagged
/// as out of scope for the first slice, now that there is a concrete need (maintenance
/// cost tracking). Documented, reviewed exception to the "every table has audit/tenant
/// columns" rule (04_DATABASE_GUIDELINES.md §3), same reasoning as Manufacturing's
/// WorkOrderComponent/BomLine — a line's lifecycle is owned entirely by its parent
/// aggregate.
///
/// ProductId is an opaque reference into Inventory's own Product aggregate
/// (03_SYSTEM_ARCHITECTURE.md §2) — never existence-validated here, same convention as
/// every other cross-module product reference (e.g. Manufacturing's
/// WorkOrderComponent.ComponentProductId). WarehouseId is likewise an opaque reference
/// into the Warehouse module — required (not optional) because it is what lets
/// Inventory's consumer actually post a real Stock Ledger movement
/// (InventoryLedgerEntry.RecordAdjustment requires a non-empty warehouse id); Asset
/// deliberately has no WarehouseId of its own (see Asset's doc comment), so the
/// warehouse a part is drawn from must be supplied at the point of usage, same as
/// AdjustStockCommand.WarehouseId is caller-supplied rather than looked up.
///
/// UnitCost is caller-supplied, never looked up from Inventory — same "no automatic
/// Inventory cost lookup" convention as Manufacturing's BomLine.UnitCost: Inventory does
/// not yet expose a queryable "current cost" for a product, and guessing at a valuation
/// method (FIFO vs weighted average) here would bake in an assumption this slice has no
/// business making. Zero when omitted.
/// </summary>
public sealed class SparePartUsage
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public decimal QuantityUsed { get; private set; }
    public decimal UnitCost { get; private set; }
    public DateTimeOffset UsedAt { get; private set; }

    private SparePartUsage() { }

    internal static SparePartUsage Create(Guid productId, Guid warehouseId, decimal quantityUsed, decimal unitCost)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse id is required.", nameof(warehouseId));
        if (quantityUsed <= 0)
            throw new ArgumentException("Quantity used must be greater than zero.", nameof(quantityUsed));
        if (unitCost < 0)
            throw new ArgumentException("Unit cost cannot be negative.", nameof(unitCost));

        return new SparePartUsage
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            QuantityUsed = quantityUsed,
            UnitCost = unitCost,
            UsedAt = DateTimeOffset.UtcNow,
        };
    }
}
