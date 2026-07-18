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

    /// <summary>
    /// Optional cross-module reference to Finance's TaxRate aggregate (opaque, never
    /// existence-validated here — same convention as ProductId). When set,
    /// <see cref="TaxAmount"/> carries the tax computed for this line's net total; the
    /// amount is supplied by the caller (via Finance's CalculateLineTaxQuery) rather
    /// than derived here, keeping tax computation on the Finance side that owns the
    /// rate. TaxAmount is 0 when no tax applies.
    /// </summary>
    public Guid? TaxRateId { get; private set; }
    public decimal TaxAmount { get; private set; }

    private PurchaseOrderLine() { }

    internal static PurchaseOrderLine Create(Guid productId, decimal quantity, decimal unitPrice, Guid? taxRateId = null, decimal taxAmount = 0m)
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

        return new PurchaseOrderLine
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

    /// <summary>
    /// Adds to the running received quantity. Deliberately does not reject
    /// over-receipt (quantityReceived pushing ReceivedQuantity past Quantity) —
    /// that is a real-world occurrence (supplier ships extra), not a data error,
    /// and rejecting it would drop a legitimate goods receipt on the floor.
    ///
    /// <b>Reviewed 2026-07-17 (a proposed over-receipt guard was considered and
    /// rejected):</b> this method is invoked from GoodsReceiptLineReceivedConsumer,
    /// an at-least-once Kafka consumer. Throwing here would not merely block extra
    /// shipments — it would fail the consumer, leaving the event un-acked and
    /// redelivered forever (a poison message). Any future over-receipt control must
    /// therefore be a tolerance/exception-flag on the receipt, decided upstream, not
    /// a hard throw on this event-driven path.
    /// </summary>
    internal void RecordReceipt(decimal quantityReceived)
    {
        if (quantityReceived <= 0)
            throw new ArgumentException("Received quantity must be greater than zero.", nameof(quantityReceived));

        ReceivedQuantity += quantityReceived;
    }
}
