namespace FusionOS.Modules.Sales.Domain.Invoices;

/// <summary>
/// A line item within an Invoice aggregate. Documented, reviewed exception to the
/// "every table has audit/tenant columns" rule (04_DATABASE_GUIDELINES.md §3),
/// same reasoning as SalesOrderLine/PurchaseOrderLine/GoodsReceiptLine/
/// JournalEntryLine: a line's lifecycle is owned entirely by its parent Invoice.
/// ProductId is an opaque reference into Inventory's Product aggregate
/// (03_SYSTEM_ARCHITECTURE.md §2) — no cross-module foreign key.
/// </summary>
public sealed class InvoiceLine
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }

    private InvoiceLine() { }

    internal static InvoiceLine Create(Guid productId, decimal quantity, decimal unitPrice)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        return new InvoiceLine
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            LineTotal = quantity * unitPrice,
        };
    }
}
