namespace FusionOS.Modules.Warehouse.Domain.PickLists;

/// <summary>
/// Plain child class (not TenantAggregateRoot), same shape as GoodsReceiptLine/SalesOrderLine —
/// private setters, an `internal static Create`, only ever constructed by the parent aggregate.
/// Unlike those two lines (which are immutable once created, since GoodsReceipt/SalesOrder are
/// create-only-then-read aggregates), a PickListLine genuinely needs an in-place mutator: a pick
/// list is a single document progressively fulfilled over time, not a new document per change — so
/// `RecordPicked` is a deliberate, documented deviation from the GoodsReceiptLine/SalesOrderLine
/// immutability convention, not an oversight.
/// </summary>
public sealed class PickListLine
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? BinId { get; private set; }
    public decimal QuantityToPick { get; private set; }
    public decimal QuantityPicked { get; private set; }

    public bool IsFullyPicked => QuantityPicked >= QuantityToPick;

    private PickListLine() { }

    internal static PickListLine Create(Guid productId, Guid? binId, decimal quantityToPick)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (quantityToPick <= 0)
            throw new ArgumentException("Quantity to pick must be greater than zero.", nameof(quantityToPick));

        return new PickListLine
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            BinId = binId,
            QuantityToPick = quantityToPick,
            QuantityPicked = 0,
        };
    }

    /// <summary>
    /// Records the total quantity picked so far for this line — an absolute value the caller
    /// re-submits each time (same "record what's true now" style as CycleCount.RecordCount),
    /// not an incremental delta, so a corrected re-entry never double-counts.
    /// </summary>
    internal void RecordPicked(decimal quantityPicked)
    {
        if (quantityPicked < 0)
            throw new ArgumentException("Quantity picked cannot be negative.", nameof(quantityPicked));
        if (quantityPicked > QuantityToPick)
            throw new ArgumentException($"Quantity picked ({quantityPicked}) cannot exceed this line's quantity to pick ({QuantityToPick}).", nameof(quantityPicked));

        QuantityPicked = quantityPicked;
    }
}
