namespace FusionOS.Modules.Procurement.Domain.PurchaseOrders;

/// <summary>
/// A line item within a PurchaseOrder aggregate. Documented, reviewed exception
/// to the "every table has audit/tenant columns" rule (04_DATABASE_GUIDELINES.md
/// §3): a line's lifecycle, company, and audit trail are entirely owned by its
/// parent PurchaseOrder — it is never queried, created, or modified independently
/// of the header, so per-line duplication of those columns would be redundant.
/// ProductId is an opaque reference into Inventory's Product aggregate
/// (03_SYSTEM_ARCHITECTURE.md §2) — no cross-module foreign key.
/// </summary>
public sealed class PurchaseOrderLine
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }

    /// <summary>
    /// Cumulative quantity received against this line so far, kept in sync by
    /// Procurement's GoodsReceiptLineReceivedConsumer reacting to Warehouse's
    /// GoodsReceiptLineReceived event (03_SYSTEM_ARCHITECTURE.md §4.2). Not
    /// itself audited/tenant-scoped, same reasoning as the rest of this line.
    /// </summary>
    public decimal ReceivedQuantity { get; private set; }

    public bool IsFullyReceived => ReceivedQuantity >= Quantity;

    private PurchaseOrderLine() { }

    internal static PurchaseOrderLine Create(Guid productId, decimal quantity, decimal unitPrice)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        return new PurchaseOrderLine
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            LineTotal = quantity * unitPrice,
        };
    }

    /// <summary>
    /// Adds to the running received quantity. Deliberately does not reject
    /// over-receipt (quantityReceived pushing ReceivedQuantity past Quantity) —
    /// that is a real-world occurrence (supplier ships extra), not a data error,
    /// and rejecting it would drop a legitimate goods receipt on the floor.
    /// </summary>
    internal void RecordReceipt(decimal quantityReceived)
    {
        if (quantityReceived <= 0)
            throw new ArgumentException("Received quantity must be greater than zero.", nameof(quantityReceived));

        ReceivedQuantity += quantityReceived;
    }
}
