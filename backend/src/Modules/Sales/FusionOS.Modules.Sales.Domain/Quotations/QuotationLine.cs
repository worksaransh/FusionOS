namespace FusionOS.Modules.Sales.Domain.Quotations;

/// <summary>
/// Same documented exception as SalesOrderLine/InvoiceLine/CreditNoteLine
/// (04_DATABASE_GUIDELINES.md §3): lifecycle/audit is owned entirely by the
/// parent Quotation. ProductId is an opaque reference into Inventory's Product
/// aggregate (03_SYSTEM_ARCHITECTURE.md §2) — no cross-module foreign key.
/// </summary>
public sealed class QuotationLine
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }

    private QuotationLine() { }

    internal static QuotationLine Create(Guid productId, decimal quantity, decimal unitPrice)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        return new QuotationLine
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            LineTotal = quantity * unitPrice,
        };
    }
}
