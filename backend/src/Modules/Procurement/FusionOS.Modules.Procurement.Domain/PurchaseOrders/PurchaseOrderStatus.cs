namespace FusionOS.Modules.Procurement.Domain.PurchaseOrders;

/// <summary>Stored as text via EF value conversion, never a native PostgreSQL enum — 04_DATABASE_GUIDELINES.md §10.</summary>
public enum PurchaseOrderStatus
{
    Draft,
    Approved,

    /// <summary>At least one line has a non-zero ReceivedQuantity but not every line is fully received yet.</summary>
    PartiallyReceived,

    /// <summary>Every line's ReceivedQuantity has reached (or exceeded) its ordered Quantity.</summary>
    FullyReceived,

    /// <summary>
    /// Submitted into Core's generic multi-step Approval workflow engine
    /// (SubmitPurchaseOrderForApprovalCommand) and awaiting a decision there.
    /// Added additively (2026-07-22) alongside the pre-existing simple
    /// Draft-to-Approved path via ApprovePurchaseOrderCommand, which still
    /// works exactly as before for whatever calls it directly.
    /// </summary>
    PendingApproval,

    /// <summary>
    /// Rejected, either directly (Reject()) or via a rejected step in Core's
    /// Approval workflow engine routed back through
    /// ApprovalRequestRejectedConsumer. Terminal — a rejected PO is not
    /// resubmitted in this phase; a new PurchaseOrder would be created instead.
    /// </summary>
    Rejected,

    /// <summary>
    /// Cancelled directly by a user via CancelPurchaseOrderCommand (Phase 10
    /// remediation, 2026-07-27) — deliberately kept distinct from Rejected:
    /// Rejected means an approver turned it down through the approval
    /// workflow, Cancelled means the business decided to void it before
    /// completion for its own reasons (wrong supplier, no longer needed,
    /// etc.), independent of any approval decision. Terminal, same as
    /// Rejected — a cancelled PO is not resurrected in this phase; a new
    /// PurchaseOrder would be created instead.
    /// </summary>
    Cancelled,
}
