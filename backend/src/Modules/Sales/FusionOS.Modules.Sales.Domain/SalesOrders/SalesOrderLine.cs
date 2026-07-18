namespace FusionOS.Modules.Sales.Domain.SalesOrders;

/// <summary>
/// Same documented exception as Procurement's PurchaseOrderLine
/// (04_DATABASE_GUIDELINES.md §3): lifecycle/audit is owned entirely by the
/// parent SalesOrder. ProductId is an opaque reference into Inventory's
/// Product aggregate (03_SYSTEM_ARCHITECTURE.md §2).
/// </summary>
public sealed class SalesOrderLine
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public decimal LineTotal { get; private set; }

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
}
