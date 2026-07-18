namespace FusionOS.Modules.Manufacturing.Domain.BillOfMaterials;

/// <summary>
/// One component line of a <see cref="BillOfMaterials"/>: an Inventory Product and how
/// many of it a single unit of the manufactured product consumes. Documented, reviewed
/// exception to the "every table has audit/tenant columns" rule
/// (04_DATABASE_GUIDELINES.md §3), same reasoning as PurchaseOrderLine/JournalEntryLine:
/// a line's lifecycle is owned entirely by its parent aggregate. ComponentProductId is an
/// opaque reference into Inventory's Product aggregate (03_SYSTEM_ARCHITECTURE.md §2) — no
/// cross-module foreign key.
/// </summary>
public sealed class BomLine
{
    public Guid Id { get; private set; }
    public Guid ComponentProductId { get; private set; }
    public decimal Quantity { get; private set; }

    private BomLine() { }

    internal static BomLine Create(Guid componentProductId, decimal quantity)
    {
        if (componentProductId == Guid.Empty)
            throw new ArgumentException("Component product id is required.", nameof(componentProductId));
        if (quantity <= 0)
            throw new ArgumentException("Component quantity must be greater than zero.", nameof(quantity));

        return new BomLine
        {
            Id = Guid.NewGuid(),
            ComponentProductId = componentProductId,
            Quantity = quantity,
        };
    }
}
