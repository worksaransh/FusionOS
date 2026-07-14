namespace FusionOS.Modules.Sales.Domain.Dispatches;

/// <summary>
/// A line item within a Dispatch aggregate. Documented, reviewed exception to the
/// "every table has audit/tenant columns" rule (04_DATABASE_GUIDELINES.md §3),
/// same reasoning as every other line-item type in this codebase. ProductId is
/// an opaque reference into Inventory's Product aggregate (03_SYSTEM_ARCHITECTURE.md §2).
/// </summary>
public sealed class DispatchLine
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal QuantityDispatched { get; private set; }

    private DispatchLine() { }

    internal static DispatchLine Create(Guid productId, decimal quantityDispatched)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (quantityDispatched <= 0)
            throw new ArgumentException("Quantity dispatched must be greater than zero.", nameof(quantityDispatched));

        return new DispatchLine
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            QuantityDispatched = quantityDispatched,
        };
    }
}
