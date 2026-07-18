using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.Payables;

/// <summary>
/// Append-only Accounts Payable ledger — the mirror image of
/// <see cref="Receivables.ArLedgerEntry"/> (Phase M4, 2026-07-15), built as
/// Phase M8c (05_MODULE_ROADMAP.md's Phase M8 a–h Finance-depth breakdown).
/// A supplier's outstanding balance is the sum of every entry's Amount, never
/// a separately maintained running total — same "recomputing from history is
/// always correct, a cached balance can drift" reasoning as AR and the
/// Inventory ledger.
///
/// SupplierId and PurchaseOrderId are opaque references into Procurement's
/// Supplier and PurchaseOrder aggregates (03_SYSTEM_ARCHITECTURE.md §2) — no
/// cross-module foreign key, same documented pattern ArLedgerEntry uses for
/// Sales' Customer/Invoice.
///
/// <b>Scope decision (Phase M8c, 2026-07-17), documented here the same way
/// CostCenter's own doc comment documents its deliberate scope-out:</b>
/// Procurement has no "Supplier Invoice"/"Bill" aggregate today — only
/// PurchaseOrder, RFQ, Supplier, SupplierScorecard, and SupplierContract (see
/// backend/src/Modules/Procurement). Inventing a full Bill/Vendor-Invoice
/// aggregate as part of this slice would be significant scope creep beyond
/// "build the AP ledger" — a real design decision (exactly when a PO becomes
/// payable — on approval? on goods receipt? on a separate three-way match?)
/// that belongs to its own separately-scoped slice, not this one. There is
/// also, consequently, no domain event equivalent to Sales' InvoiceIssued for
/// Finance to react to for an ad-hoc bill, so for a bill with no goods receipt
/// behind it (or no PO at all), <see cref="RecordBillCharge"/> is still invoked
/// directly by a manual RecordBillChargeCommand — a Finance user recording
/// "we owe supplier X this much for PO Y" (or for no PO at all) by hand. This
/// mirrors how AR itself started: RecordPayment has always been a manual
/// command (no integration event backs a customer payment either), and
/// AR only grew a consumer-fed charge side later.
///
/// <b>Blocker resolved (2026-07-18, Step-2 integration-gap pass):</b> the
/// auto-charge-from-goods-receipt follow-up was blocked because Warehouse's
/// GoodsReceiptLineReceived event carries ProductId/QuantityReceived/
/// UnitCost/PurchaseOrderId but NOT SupplierId, and RecordBillCharge requires
/// a supplier. Rather than give Finance a cross-module lookup back into
/// Procurement's PurchaseOrder (this codebase's modules never take a
/// compile-time or query-time dependency on another module — eventing only),
/// Procurement itself — which already consumes GoodsReceiptLineReceived to
/// advance received-status, and already owns SupplierId — now re-publishes a
/// supplier-attributed PurchaseOrderGoodsReceiptCosted event whenever the
/// triggering receipt line carried a real UnitCost. Finance's
/// PurchaseOrderGoodsReceiptCostedConsumer reacts to that event and calls
/// RecordBillCharge automatically. A receipt with no UnitCost, or a bill with
/// no purchase order at all, is unaffected and still needs the manual command.
///
/// Because a bill charge is not always tied to one specific purchase order
/// (an ad-hoc supplier bill has no PO at all), <see cref="PurchaseOrderId"/>
/// is optional — unlike ArLedgerEntry.InvoiceId, which is mandatory because
/// every AR charge always originates from one specific Sales invoice. This
/// also means the "don't let this transaction exceed what's outstanding"
/// guard RecordPaymentCommandHandler enforces is scoped to the *supplier's*
/// total outstanding balance (SumAmountAsync) rather than to one purchase
/// order's balance the way RecordPaymentCommandHandler on the AR side scopes
/// its guard to one specific invoice (SumAmountByInvoiceAsync) — a per-PO
/// guard isn't always meaningful here since PurchaseOrderId can be null.
/// </summary>
public sealed class ApLedgerEntry : TenantAggregateRoot
{
    public Guid SupplierId { get; private set; }
    public Guid? PurchaseOrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = default!;
    public DateTimeOffset TransactionDate { get; private set; }

    private ApLedgerEntry() { }

    /// <summary>
    /// Records a bill charge — money owed to a supplier — as a positive
    /// ledger entry. PurchaseOrderId is optional (see class doc comment);
    /// Description is supplied by the caller (RecordBillChargeCommandHandler)
    /// rather than auto-generated the way ArLedgerEntry.RecordInvoiceCharge
    /// derives its own from an invoice id, since a manually-entered bill has
    /// no invoice-issued event payload to summarize from.
    /// </summary>
    public static ApLedgerEntry RecordBillCharge(Guid companyId, Guid supplierId, Guid? purchaseOrderId, decimal amount, string description)
    {
        if (supplierId == Guid.Empty)
            throw new ArgumentException("Supplier id is required.", nameof(supplierId));
        if (amount <= 0)
            throw new ArgumentException("A bill charge must be a positive amount.", nameof(amount));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A bill charge description is required.", nameof(description));

        return new ApLedgerEntry
        {
            CompanyId = companyId,
            SupplierId = supplierId,
            PurchaseOrderId = purchaseOrderId,
            Amount = amount,
            Description = description.Trim(),
            TransactionDate = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Records a payment made to a supplier as a negative ledger entry —
    /// since the balance is always the sum of every entry's Amount (see class
    /// doc comment), a payment just needs the opposite sign of a charge, not
    /// a separate "reduce balance" operation. Mirrors
    /// ArLedgerEntry.RecordPayment's shape exactly, except PurchaseOrderId is
    /// optional here (see class doc comment on why the overpay guard is
    /// scoped to the supplier, not a specific purchase order).
    /// RecordPaymentCommandHandler is responsible for checking the payment
    /// doesn't exceed the supplier's current outstanding balance before
    /// calling this factory — this factory only enforces shape, not that
    /// cross-entry business rule (mirrors RecordBillCharge/AR's
    /// RecordPayment, which also only validate their own inputs).
    /// </summary>
    public static ApLedgerEntry RecordPayment(Guid companyId, Guid supplierId, Guid? purchaseOrderId, decimal amount, string? reference, DateTimeOffset? transactionDate = null)
    {
        if (supplierId == Guid.Empty)
            throw new ArgumentException("Supplier id is required.", nameof(supplierId));
        if (amount <= 0)
            throw new ArgumentException("A payment amount must be positive.", nameof(amount));

        var description = string.IsNullOrWhiteSpace(reference)
            ? $"Payment to supplier {supplierId}"
            : $"Payment to supplier {supplierId} ({reference.Trim()})";

        return new ApLedgerEntry
        {
            CompanyId = companyId,
            SupplierId = supplierId,
            PurchaseOrderId = purchaseOrderId,
            Amount = -amount,
            Description = description,
            TransactionDate = transactionDate ?? DateTimeOffset.UtcNow,
        };
    }
}
