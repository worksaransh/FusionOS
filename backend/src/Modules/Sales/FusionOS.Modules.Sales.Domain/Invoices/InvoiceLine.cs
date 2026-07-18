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

    /// <summary>
    /// Optional cross-module reference to Finance's TaxRate aggregate (opaque, never
    /// existence-validated here — same convention as ProductId, since validating it
    /// would require a Sales→Finance project reference this module doesn't take). When
    /// set, <see cref="TaxAmount"/> carries the tax computed for this line's net total;
    /// the amount itself is supplied by the caller (via Finance's CalculateLineTaxQuery)
    /// rather than derived here, keeping tax computation on the Finance side that owns
    /// the rate. TaxAmount is 0 when no tax applies.
    /// </summary>
    public Guid? TaxRateId { get; private set; }
    public decimal TaxAmount { get; private set; }

    private InvoiceLine() { }

    internal static InvoiceLine Create(Guid productId, decimal quantity, decimal unitPrice, Guid? taxRateId = null, decimal taxAmount = 0m)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));
        if (taxRateId == Guid.Empty)
            throw new ArgumentException("Tax rate id, when supplied, cannot be empty.", nameof(taxRateId));
        if (taxAmount < 0)
            throw new ArgumentException("Tax amount cannot be negative.", nameof(taxAmount));
        if (taxRateId is null && taxAmount != 0m)
            throw new ArgumentException("Tax amount must be zero when no tax rate is set.", nameof(taxAmount));

        return new InvoiceLine
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            LineTotal = quantity * unitPrice,
            TaxRateId = taxRateId,
            TaxAmount = taxAmount,
        };
    }
}
