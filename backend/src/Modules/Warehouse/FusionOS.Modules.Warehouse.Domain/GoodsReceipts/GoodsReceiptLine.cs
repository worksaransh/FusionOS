namespace FusionOS.Modules.Warehouse.Domain.GoodsReceipts;

/// <summary>
/// A line item within a GoodsReceipt aggregate. Documented, reviewed exception to
/// the "every table has audit/tenant columns" rule (04_DATABASE_GUIDELINES.md §3),
/// same reasoning as PurchaseOrderLine/SalesOrderLine: a line's lifecycle is owned
/// entirely by its parent GoodsReceipt. ProductId is an opaque reference into
/// Inventory's Product aggregate (03_SYSTEM_ARCHITECTURE.md §2) — no cross-module
/// foreign key.
/// </summary>
public sealed class GoodsReceiptLine
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal QuantityReceived { get; private set; }
    public decimal? UnitCost { get; private set; }

    private GoodsReceiptLine() { }

    internal static GoodsReceiptLine Create(Guid productId, decimal quantityReceived, decimal? unitCost)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (quantityReceived <= 0)
            throw new ArgumentException("Quantity received must be greater than zero.", nameof(quantityReceived));
        if (unitCost is < 0)
            throw new ArgumentException("Unit cost cannot be negative.", nameof(unitCost));

        return new GoodsReceiptLine
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            QuantityReceived = quantityReceived,
            UnitCost = unitCost,
        };
    }
}
