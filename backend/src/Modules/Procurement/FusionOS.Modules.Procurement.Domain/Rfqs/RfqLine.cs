namespace FusionOS.Modules.Procurement.Domain.Rfqs;

/// <summary>
/// Same documented exception as PurchaseOrderLine/QuotationLine (04_DATABASE_GUIDELINES.md
/// §3): lifecycle/audit is owned entirely by the parent RequestForQuotation. ProductId is
/// an opaque reference into Inventory's Product aggregate (03_SYSTEM_ARCHITECTURE.md §2) —
/// no cross-module foreign key. Deliberately has no UnitPrice — see RfqLineInput.
/// </summary>
public sealed class RfqLine
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }

    private RfqLine() { }

    internal static RfqLine Create(Guid productId, decimal quantity)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        return new RfqLine
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = quantity,
        };
    }
}
