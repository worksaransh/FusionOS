namespace FusionOS.Modules.Sales.Domain.PriceLists;

/// <summary>
/// One product's override price within a PriceList. Same documented exception
/// to per-row audit/tenant columns as every other line entity in this codebase
/// (04_DATABASE_GUIDELINES.md §3) — lifecycle is owned entirely by the parent
/// PriceList. ProductId is an opaque reference into Inventory's Product
/// aggregate — no cross-module foreign key.
/// </summary>
public sealed class PriceListEntry
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal UnitPrice { get; private set; }

    private PriceListEntry() { }

    internal static PriceListEntry Create(Guid productId, decimal unitPrice)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        return new PriceListEntry
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            UnitPrice = unitPrice,
        };
    }
}
