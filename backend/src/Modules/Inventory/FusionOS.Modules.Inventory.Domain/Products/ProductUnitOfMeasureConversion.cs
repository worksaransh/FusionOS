namespace FusionOS.Modules.Inventory.Domain.Products;

/// <summary>
/// One alternate unit of measure a Product can be ordered/received/dispatched in,
/// and its conversion factor back to the Product's own base UnitOfMeasure (M9
/// remaining — Multi-UOM, 2026-07-16). E.g. base UOM "PCS", AlternateUnitOfMeasure
/// "BOX", ConversionFactor 12 means 1 BOX == 12 PCS.
///
/// Same "line item owned entirely by its parent aggregate" shape as
/// GoodsReceiptLine/PurchaseOrderLine/SalesOrderLine — no audit/tenant columns of
/// its own (04_DATABASE_GUIDELINES.md §3 documented exception), lifecycle owned by
/// Product, mutators are internal (only Product itself calls them).
///
/// The actual unit conversion math is NOT performed inside this module for
/// receive/dispatch flows — Warehouse and Sales are separate modules with no
/// cross-module read of Inventory's Product (03_SYSTEM_ARCHITECTURE.md §2), so
/// the caller (frontend, or whichever command constructs a GoodsReceiptLine/
/// DispatchLine) is responsible for resolving an alternate-UOM quantity into the
/// Product's base UOM — by reading these conversions via GetProductByIdQuery —
/// before it reaches the ledger, which always stores quantities in the Product's
/// base UOM for valuation/costing consistency. This is the same "caller supplies
/// the data" restraint CycleCount.Start uses for its system-quantity snapshot.
/// </summary>
public sealed class ProductUnitOfMeasureConversion
{
    public Guid Id { get; private set; }
    public string AlternateUnitOfMeasure { get; private set; } = default!;
    public decimal ConversionFactor { get; private set; }

    private ProductUnitOfMeasureConversion() { }

    internal static ProductUnitOfMeasureConversion Create(string alternateUnitOfMeasure, decimal conversionFactor)
    {
        if (string.IsNullOrWhiteSpace(alternateUnitOfMeasure))
            throw new ArgumentException("Alternate unit of measure is required.", nameof(alternateUnitOfMeasure));
        if (conversionFactor <= 0)
            throw new ArgumentException("Conversion factor must be greater than zero.", nameof(conversionFactor));

        return new ProductUnitOfMeasureConversion
        {
            Id = Guid.NewGuid(),
            AlternateUnitOfMeasure = alternateUnitOfMeasure.Trim().ToUpperInvariant(),
            ConversionFactor = conversionFactor,
        };
    }
}
