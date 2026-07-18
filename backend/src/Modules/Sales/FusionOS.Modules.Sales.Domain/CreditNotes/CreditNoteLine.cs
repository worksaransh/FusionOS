namespace FusionOS.Modules.Sales.Domain.CreditNotes;

/// <summary>
/// A line item within a CreditNote aggregate. Documented, reviewed exception to the
/// "every table has audit/tenant columns" rule (04_DATABASE_GUIDELINES.md §3),
/// same reasoning as InvoiceLine/SalesOrderLine/PurchaseOrderLine/GoodsReceiptLine:
/// a line's lifecycle is owned entirely by its parent CreditNote. ProductId is an
/// opaque reference into Inventory's Product aggregate (03_SYSTEM_ARCHITECTURE.md §2)
/// — no cross-module foreign key.
/// </summary>
public sealed class CreditNoteLine
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }

    private CreditNoteLine() { }

    internal static CreditNoteLine Create(Guid productId, decimal quantity, decimal unitPrice)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        return new CreditNoteLine
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            LineTotal = quantity * unitPrice,
        };
    }
}
