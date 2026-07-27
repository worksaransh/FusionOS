namespace FusionOS.Modules.Sales.Domain.Returns;

/// <summary>
/// A line item within a SalesReturn aggregate. Documented, reviewed exception to the
/// "every table has audit/tenant columns" rule (04_DATABASE_GUIDELINES.md §3), same
/// reasoning as InvoiceLine/CreditNoteLine/SalesOrderLine: a line's lifecycle is owned
/// entirely by its parent SalesReturn. ProductId is an opaque reference into
/// Inventory's Product aggregate (03_SYSTEM_ARCHITECTURE.md §2) — no cross-module
/// foreign key.
///
/// ConditionNotes and Restocked are both null/false until the parent SalesReturn is
/// Completed — the physical condition of returned goods, and whether they end up
/// restocked, can only be known once the goods have actually arrived back and been
/// inspected (see SalesReturn.Complete).
/// </summary>
public sealed class SalesReturnLine
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public string? ConditionNotes { get; private set; }
    public bool Restocked { get; private set; }

    private SalesReturnLine() { }

    internal static SalesReturnLine Create(Guid productId, decimal quantity)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        return new SalesReturnLine
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = quantity,
        };
    }

    /// <summary>Called only from SalesReturn.Complete, once per line, with the inspection results for the physically returned goods.</summary>
    internal void ApplyCompletion(bool restocked, string? conditionNotes)
    {
        Restocked = restocked;
        ConditionNotes = conditionNotes;
    }
}
