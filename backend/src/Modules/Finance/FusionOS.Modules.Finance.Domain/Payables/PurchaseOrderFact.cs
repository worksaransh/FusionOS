using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.Payables;

/// <summary>
/// Finance's local read-model of purchase-order facts it has learned from
/// Procurement's integration events — the missing piece that unblocks the
/// Procurement three-way match (PROJECT_TRACKER.md §3, "blocked on Accounts
/// Payable not existing"; AP now exists as <see cref="ApLedgerEntry"/>).
/// One row per (CompanyId, PurchaseOrderId). Built and updated exclusively by
/// PurchaseOrderApprovedConsumer (sets <see cref="OrderedAmount"/> from the
/// PurchaseOrderApproved event's TotalAmount) and
/// PurchaseOrderGoodsReceiptCostedConsumer (accumulates
/// <see cref="ReceivedAmount"/> from each costed receipt's LineAmount) — never
/// by a user-facing command. PurchaseOrderId and SupplierId remain opaque
/// references into Procurement (03_SYSTEM_ARCHITECTURE.md §2): Finance never
/// queries Procurement's tables or types; everything here arrived on the wire,
/// same reviewed pattern as every consumer in this codebase.
///
/// <b>Three-way-match policy (documented here once, referenced by the
/// handlers that enforce it):</b> the match is enforced against facts received
/// so far — no more, no less — because cross-module data flows only through
/// eventually-consistent Kafka events:
/// <list type="bullet">
/// <item><b>PO leg:</b> <see cref="OrderedAmount"/> is nullable because a
/// PurchaseOrderGoodsReceiptCosted event can be consumed before its
/// PurchaseOrderApproved event (separate topics, no cross-topic ordering
/// guarantee). While null, the ordered-amount ceiling simply cannot be
/// enforced and is skipped.</item>
/// <item><b>GRN leg:</b> <see cref="ReceivedAmount"/> == 0 is indistinguishable
/// from "the receipt events just haven't been consumed yet," so the
/// received-amount ceiling is only enforced once at least one costed receipt
/// fact has arrived (ReceivedAmount &gt; 0). A bill recorded before its
/// receipt facts arrive is validated only against the PO fact (if present) —
/// honest eventual consistency, not fake exactness.</item>
/// <item><b>No fact row at all:</b> Finance has learned nothing about that
/// purchase order (events predate these consumers, or the PO was never
/// approved with these consumers running), so the charge is accepted
/// unvalidated — exactly the pre-three-way-match behavior for that PO.</item>
/// <item><b>Uncosted receipts:</b> Procurement only publishes
/// PurchaseOrderGoodsReceiptCosted when the Warehouse receipt line carried a
/// real UnitCost (see PurchaseOrder.RecordGoodsReceipt), so an uncosted
/// receipt never contributes to ReceivedAmount — its bill remains a manual,
/// PO-leg-only-validated entry, same as before.</item>
/// </list>
/// Mutable by design (unlike the append-only ApLedgerEntry): this is a
/// projection of another module's state, not a financial record of ours —
/// same reasoning as FinanceSettings being an updatable singleton while the
/// ledgers stay append-only.
/// </summary>
public sealed class PurchaseOrderFact : TenantAggregateRoot
{
    public Guid PurchaseOrderId { get; private set; }
    public Guid SupplierId { get; private set; }

    /// <summary>Total ordered amount from PurchaseOrderApproved; null until that event has been consumed (see class doc comment's PO-leg policy).</summary>
    public decimal? OrderedAmount { get; private set; }

    /// <summary>Running sum of every costed goods-receipt LineAmount consumed so far (see class doc comment's GRN-leg policy).</summary>
    public decimal ReceivedAmount { get; private set; }

    private PurchaseOrderFact() { }

    /// <summary>Creates the fact when PurchaseOrderApproved is the first event Finance sees for this PO — the normal ordering.</summary>
    public static PurchaseOrderFact FromApproval(Guid companyId, Guid purchaseOrderId, Guid supplierId, decimal orderedAmount)
    {
        if (purchaseOrderId == Guid.Empty)
            throw new ArgumentException("Purchase order id is required.", nameof(purchaseOrderId));
        if (supplierId == Guid.Empty)
            throw new ArgumentException("Supplier id is required.", nameof(supplierId));
        if (orderedAmount < 0)
            throw new ArgumentException("An ordered amount cannot be negative.", nameof(orderedAmount));

        return new PurchaseOrderFact
        {
            CompanyId = companyId,
            PurchaseOrderId = purchaseOrderId,
            SupplierId = supplierId,
            OrderedAmount = orderedAmount,
            ReceivedAmount = 0m,
        };
    }

    /// <summary>
    /// Creates the fact when a PurchaseOrderGoodsReceiptCosted event arrives
    /// before its PurchaseOrderApproved event — OrderedAmount stays null until
    /// the approval is consumed (see class doc comment's PO-leg policy).
    /// </summary>
    public static PurchaseOrderFact FromGoodsReceipt(Guid companyId, Guid purchaseOrderId, Guid supplierId, decimal receivedLineAmount)
    {
        if (purchaseOrderId == Guid.Empty)
            throw new ArgumentException("Purchase order id is required.", nameof(purchaseOrderId));
        if (supplierId == Guid.Empty)
            throw new ArgumentException("Supplier id is required.", nameof(supplierId));
        if (receivedLineAmount < 0)
            throw new ArgumentException("A received line amount cannot be negative.", nameof(receivedLineAmount));

        return new PurchaseOrderFact
        {
            CompanyId = companyId,
            PurchaseOrderId = purchaseOrderId,
            SupplierId = supplierId,
            OrderedAmount = null,
            ReceivedAmount = receivedLineAmount,
        };
    }

    /// <summary>Sets the ordered amount when the approval event arrives after a receipt event already created this fact. Idempotent-safe: Procurement approves a PO exactly once (Draft → Approved), so this never legitimately fires twice with different values; redeliveries are already filtered by the processed-event dedupe guard.</summary>
    public void ApplyApproval(decimal orderedAmount)
    {
        if (orderedAmount < 0)
            throw new ArgumentException("An ordered amount cannot be negative.", nameof(orderedAmount));

        OrderedAmount = orderedAmount;
    }

    /// <summary>Accumulates one more costed receipt line into the running received total.</summary>
    public void ApplyGoodsReceipt(decimal receivedLineAmount)
    {
        if (receivedLineAmount < 0)
            throw new ArgumentException("A received line amount cannot be negative.", nameof(receivedLineAmount));

        ReceivedAmount += receivedLineAmount;
    }
}
