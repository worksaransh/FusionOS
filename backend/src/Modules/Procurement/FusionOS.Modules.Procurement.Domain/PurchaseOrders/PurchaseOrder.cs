using FusionOS.SharedKernel;
using FusionOS.Modules.Procurement.Domain.PurchaseOrders.Events;

namespace FusionOS.Modules.Procurement.Domain.PurchaseOrders;

/// <summary>
/// The next slice after Supplier (05_MODULE_ROADMAP.md Phase 1). RFQ/supplier
/// comparison come later; this slice covers PO creation, the approval workflow
/// named explicitly in the PRD ("Approvals"), and receiving status kept in sync
/// by GoodsReceiptLineReceivedConsumer reacting to Warehouse's goods receipts
/// (03_SYSTEM_ARCHITECTURE.md §4.2).
/// </summary>
public sealed class PurchaseOrder : TenantAggregateRoot
{
    private readonly List<PurchaseOrderLine> _lines = new();

    public Guid SupplierId { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public DateTimeOffset OrderDate { get; private set; }
    public IReadOnlyList<PurchaseOrderLine> Lines => _lines.AsReadOnly();
    public decimal TotalAmount => _lines.Sum(l => l.LineTotal);

    private PurchaseOrder() { }

    public static PurchaseOrder Create(Guid companyId, Guid supplierId, IReadOnlyCollection<PurchaseOrderLineInput> lines)
    {
        if (supplierId == Guid.Empty)
            throw new ArgumentException("Supplier id is required.", nameof(supplierId));
        if (lines is null || lines.Count == 0)
            throw new ArgumentException("A purchase order must have at least one line.", nameof(lines));

        var order = new PurchaseOrder
        {
            CompanyId = companyId,
            SupplierId = supplierId,
            Status = PurchaseOrderStatus.Draft,
            OrderDate = DateTimeOffset.UtcNow,
        };

        foreach (var line in lines)
            order._lines.Add(PurchaseOrderLine.Create(line.ProductId, line.Quantity, line.UnitPrice));

        order.Raise(new PurchaseOrderCreated(order.Id, companyId, supplierId, order.TotalAmount));
        return order;
    }

    /// <summary>Raises PurchaseOrderApproved — the event Inventory (expected receipt) and Finance (AP accrual) consume per 03_SYSTEM_ARCHITECTURE.md §4.2's event catalog, once cross-module consumption is wired up.</summary>
    public void Approve()
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException($"Only a Draft purchase order can be approved (current status: {Status}).");

        Status = PurchaseOrderStatus.Approved;
        Raise(new PurchaseOrderApproved(Id, CompanyId, SupplierId, TotalAmount));
    }

    /// <summary>
    /// Applies one received line from a Warehouse goods receipt to the matching
    /// PurchaseOrderLine (matched by ProductId — the same documented, reviewed
    /// simplification as elsewhere in this aggregate: a PO with two lines for the
    /// same product would have both receipts applied to whichever line is found
    /// first, since there is no per-line correlation id on the wire event yet).
    /// A no-op if no line matches (the receipt wasn't actually for this PO's
    /// product mix) rather than throwing, since a Kafka consumer must never fail
    /// a whole redelivery over a shape mismatch it can't fully control.
    /// </summary>
    public void RecordGoodsReceipt(Guid productId, decimal quantityReceived)
    {
        var line = _lines.FirstOrDefault(l => l.ProductId == productId);
        if (line is null)
            return;

        line.RecordReceipt(quantityReceived);

        if (_lines.All(l => l.IsFullyReceived))
            Status = PurchaseOrderStatus.FullyReceived;
        else if (_lines.Any(l => l.ReceivedQuantity > 0) && Status != PurchaseOrderStatus.FullyReceived)
            Status = PurchaseOrderStatus.PartiallyReceived;
    }
}
