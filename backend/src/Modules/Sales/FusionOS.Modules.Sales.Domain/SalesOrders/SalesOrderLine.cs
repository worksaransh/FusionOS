namespace FusionOS.Modules.Sales.Domain.SalesOrders;

/// <summary>
/// Same documented exception as Procurement's PurchaseOrderLine
/// (04_DATABASE_GUIDELINES.md §3): lifecycle/audit is owned entirely by the
/// parent SalesOrder. ProductId is an opaque reference into Inventory's
/// Product aggregate (03_SYSTEM_ARCHITECTURE.md §2).
///
/// <see cref="IsBackordered"/>/<see cref="BackorderedQuantity"/> (Phase 1
/// closeout, 2026-07-18) are a manually-set flag, not an automatic
/// availability check — SalesOrder has no WarehouseId of its own yet (only
/// Dispatch does, later in the flow) and SalesOrderConfirmed's event payload
/// carries no per-line detail, so there is no automatic way for this module
/// to know Inventory's available-to-promise at confirm time without either a
/// prohibited cross-module reference or a larger, separately-scoped event
/// redesign. A sales/warehouse user who has checked Inventory's own
/// available-to-promise (Reservations, same Phase 1 closeout pass) flags the
/// line by hand — same "manual first" restraint as AI's RecordRecommendation
/// and Integration Hub's MarkConnectorConnectionError.
/// </summary>
public sealed class SalesOrderLine
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public decimal LineTotal { get; private set; }
    public bool IsBackordered { get; private set; }
    public decimal BackorderedQuantity { get; private set; }

    private SalesOrderLine() { }

    internal static SalesOrderLine Create(Guid productId, decimal quantity, decimal unitPrice, decimal discountPercentage = 0m)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));
        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ArgumentException("Discount percentage must be between 0 and 100.", nameof(discountPercentage));

        return new SalesOrderLine
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            DiscountPercentage = discountPercentage,
            LineTotal = quantity * unitPrice * (1 - discountPercentage / 100m),
        };
    }

    internal void FlagBackordered(decimal backorderedQuantity)
    {
        if (backorderedQuantity <= 0 || backorderedQuantity > Quantity)
            throw new ArgumentException($"Backordered quantity must be greater than zero and cannot exceed the line's ordered quantity ({Quantity}).", nameof(backorderedQuantity));

        IsBackordered = true;
        BackorderedQuantity = backorderedQuantity;
    }

    internal void ClearBackorder()
    {
        IsBackordered = false;
        BackorderedQuantity = 0m;
    }
}
