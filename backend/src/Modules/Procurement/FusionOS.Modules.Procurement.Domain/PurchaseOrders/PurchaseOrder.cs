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
            order._lines.Add(PurchaseOrderLine.Create(line.ProductId, line.Quantity, line.UnitPrice, line.TaxRateId, line.TaxAmount));

        order.Raise(new PurchaseOrderCreated(order.Id, companyId, supplierId, order.TotalAmount));
        return order;
    }

    /// <summary>
    /// Raises PurchaseOrderApproved — the event Inventory (expected receipt) and Finance (AP accrual) consume per 03_SYSTEM_ARCHITECTURE.md §4.2's event catalog, once cross-module consumption is wired up.
    /// Allowed from Draft (the original, still-working simple maker-checker
    /// path via ApprovePurchaseOrderCommand) or from PendingApproval (2026-07-22,
    /// the new path: ApprovalRequestApprovedConsumer calling this once Core's
    /// generic Approval workflow engine reaches a final Approved decision).
    /// </summary>
    public void Approve()
    {
        if (Status != PurchaseOrderStatus.Draft && Status != PurchaseOrderStatus.PendingApproval)
            throw new InvalidOperationException($"Only a Draft or PendingApproval purchase order can be approved (current status: {Status}).");

        Status = PurchaseOrderStatus.Approved;
        Raise(new PurchaseOrderApproved(Id, CompanyId, SupplierId, TotalAmount));
    }

    /// <summary>
    /// Submits a Draft purchase order into Core's generic, multi-step Approval
    /// workflow engine (2026-07-22) — an additive alternative to the direct
    /// Approve() path above, not a replacement for it. Raises
    /// PurchaseOrderSubmittedForApproval on the outbox so Core's
    /// PurchaseOrderSubmittedForApprovalConsumer can create a real
    /// ApprovalRequest for it; Procurement never talks to Core's Domain/Application
    /// types directly (module boundary rule).
    ///
    /// No amount threshold is enforced here: this codebase's existing
    /// convention is to not invent business policy it hasn't been asked for
    /// (see ApprovalRequest.cs's own doc comment about "who should approve a
    /// PO over $10,000" being caller-defined, not engine-defined). Submission
    /// is simply always available to whoever holds the
    /// procurement.purchase-order.submit-for-approval permission; a future
    /// FinanceSettings-style threshold configuration can gate the *decision*
    /// to call this rather than Approve() directly, without any domain change
    /// here.
    /// </summary>
    public void SubmitForApproval(Guid requestedByUserId)
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException($"Only a Draft purchase order can be submitted for approval (current status: {Status}).");
        if (requestedByUserId == Guid.Empty)
            throw new ArgumentException("Requested-by user id is required.", nameof(requestedByUserId));

        Status = PurchaseOrderStatus.PendingApproval;
        Raise(new PurchaseOrderSubmittedForApproval(
            Id,
            CompanyId,
            requestedByUserId,
            TotalAmount,
            $"Purchase order {Id} for supplier {SupplierId} totaling {TotalAmount:0.00}"));
    }

    /// <summary>
    /// Rejects a purchase order — new (2026-07-22), closing the loop for
    /// ApprovalRequestRejectedConsumer once Core's Approval workflow engine
    /// records a rejected step against this PO. Also usable directly from
    /// Draft (e.g. a future direct-reject UI action), same as Approve()
    /// supports both a direct path and the workflow-engine path.
    /// </summary>
    public void Reject()
    {
        if (Status != PurchaseOrderStatus.Draft && Status != PurchaseOrderStatus.PendingApproval)
            throw new InvalidOperationException($"Only a Draft or PendingApproval purchase order can be rejected (current status: {Status}).");

        Status = PurchaseOrderStatus.Rejected;
        Raise(new PurchaseOrderRejected(Id, CompanyId, SupplierId, TotalAmount));
    }

    /// <summary>
    /// Cancels a purchase order directly (Phase 10 remediation, 2026-07-27,
    /// closing the long-known gap that Cancel had no domain method, command,
    /// controller route, or frontend action at all). Deliberately separate
    /// from Reject(): Reject models an approver turning the PO down through
    /// the approval workflow, Cancel models the business voiding it for its
    /// own reasons before it's complete. Allowed from Draft, PendingApproval,
    /// or Approved — anything not yet Completed (FullyReceived) and not
    /// already terminal (Cancelled/Rejected), matching the same
    /// InvalidOperationException-with-message convention as Approve()/Reject().
    /// A partially-received order is intentionally excluded: goods have
    /// already changed hands, so cancelling it outright would hide that a
    /// vendor return is the correct next step instead.
    /// </summary>
    public void Cancel()
    {
        if (Status != PurchaseOrderStatus.Draft && Status != PurchaseOrderStatus.PendingApproval && Status != PurchaseOrderStatus.Approved)
            throw new InvalidOperationException($"Only a Draft, PendingApproval, or Approved purchase order can be cancelled (current status: {Status}).");

        Status = PurchaseOrderStatus.Cancelled;
        Raise(new PurchaseOrderCancelled(Id, CompanyId, SupplierId, TotalAmount));
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
    ///
    /// <b>unitCost (2026-07-18, closes the ApLedgerEntry blocker):</b> when the
    /// triggering receipt line carried a real UnitCost, raises
    /// PurchaseOrderGoodsReceiptCosted so Finance can auto-charge Accounts
    /// Payable without ever needing a cross-module lookup back into this
    /// aggregate — SupplierId travels with the event instead. Not raised when
    /// unitCost is null (the receiving Warehouse didn't know a cost), which
    /// simply means that receipt still needs a manual RecordBillChargeCommand,
    /// same as before this event existed.
    /// </summary>
    public void RecordGoodsReceipt(Guid productId, decimal quantityReceived, decimal? unitCost = null)
    {
        var line = _lines.FirstOrDefault(l => l.ProductId == productId);
        if (line is null)
            return;

        line.RecordReceipt(quantityReceived);

        if (_lines.All(l => l.IsFullyReceived))
            Status = PurchaseOrderStatus.FullyReceived;
        else if (_lines.Any(l => l.ReceivedQuantity > 0) && Status != PurchaseOrderStatus.FullyReceived)
            Status = PurchaseOrderStatus.PartiallyReceived;

        if (unitCost.HasValue)
        {
            // Prorate the line's total tax by this receipt's share of the ordered quantity —
            // a partial receipt shouldn't carry the whole line's tax (GST filing support, 2026-07-22).
            var taxAmount = line.TaxRateId.HasValue
                ? Math.Round(line.TaxAmount * (quantityReceived / line.Quantity), 2)
                : 0m;

            Raise(new PurchaseOrderGoodsReceiptCosted(
                Id, CompanyId, SupplierId, productId, quantityReceived, unitCost.Value, quantityReceived * unitCost.Value,
                line.TaxRateId, taxAmount));
        }
    }
}
