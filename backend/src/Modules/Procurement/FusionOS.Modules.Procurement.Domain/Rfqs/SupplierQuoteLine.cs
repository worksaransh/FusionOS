namespace FusionOS.Modules.Procurement.Domain.Rfqs;

/// <summary>
/// A per-product price submitted by one supplier against one RfqLine. Quantity is
/// copied in at submission time (rather than re-read off the parent's RfqLine) so
/// this entity — and its LineTotal — is self-contained, the same reasoning
/// PurchaseOrderLine documents for owning its own LineTotal. Same
/// documented-exception-to-audit-columns rule as every other line entity in this
/// codebase: lifecycle is owned entirely by the parent SupplierQuote.
/// </summary>
public sealed class SupplierQuoteLine
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }

    private SupplierQuoteLine() { }

    internal static SupplierQuoteLine Create(Guid productId, decimal quantity, decimal unitPrice)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        return new SupplierQuoteLine
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            LineTotal = quantity * unitPrice,
        };
    }
}
