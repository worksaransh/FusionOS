using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.Receivables;

/// <summary>
/// Append-only Accounts Receivable ledger — the minimal AR slice named in
/// 05_MODULE_ROADMAP.md Phase 2 (previously "reserved for later," per
/// FinanceModule's own doc comment). Kept in sync by InvoiceIssuedConsumer
/// reacting to Sales' InvoiceIssued event (03_SYSTEM_ARCHITECTURE.md §4.2),
/// the same append-only-ledger-fed-by-a-Kafka-consumer shape as Inventory's
/// StockLedger. A customer's outstanding balance is the sum of every entry's
/// Amount, never a separately maintained running total — same reasoning as
/// the Inventory ledger: recomputing from history is always correct,
/// a cached balance can drift.
///
/// CustomerId and InvoiceId are opaque references into Sales' Customer and
/// Invoice aggregates (03_SYSTEM_ARCHITECTURE.md §2) — no cross-module
/// foreign key, same documented pattern used everywhere else in this
/// codebase for cross-module references.
///
/// "Charge" entries (positive Amount, one per issued invoice) are created by
/// InvoiceIssuedConsumer. "Payment" entries (negative Amount) are created
/// directly by RecordPaymentCommand (Phase M4, 2026-07-15) — a customer
/// payment doesn't arrive via any integration event, so it needs its own
/// command/endpoint rather than a consumer.
/// </summary>
public sealed class ArLedgerEntry : TenantAggregateRoot
{
    public Guid CustomerId { get; private set; }
    public Guid InvoiceId { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = default!;
    public DateTimeOffset TransactionDate { get; private set; }

    private ArLedgerEntry() { }

    public static ArLedgerEntry RecordInvoiceCharge(Guid companyId, Guid customerId, Guid invoiceId, decimal amount)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer id is required.", nameof(customerId));
        if (invoiceId == Guid.Empty)
            throw new ArgumentException("Invoice id is required.", nameof(invoiceId));
        if (amount <= 0)
            throw new ArgumentException("An invoice charge must be a positive amount.", nameof(amount));

        return new ArLedgerEntry
        {
            CompanyId = companyId,
            CustomerId = customerId,
            InvoiceId = invoiceId,
            Amount = amount,
            Description = $"Invoice {invoiceId}",
            TransactionDate = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Records a customer payment against a specific invoice as a negative
    /// ledger entry — since the balance is always the sum of every entry's
    /// Amount (see class doc comment), a payment just needs the opposite
    /// sign of a charge, not a separate "reduce balance" operation.
    /// RecordPaymentCommandHandler is responsible for checking the payment
    /// doesn't exceed that invoice's current outstanding balance before
    /// calling this factory — this factory only enforces shape, not that
    /// cross-entry business rule (mirrors RecordInvoiceCharge, which also
    /// only validates its own inputs).
    /// </summary>
    public static ArLedgerEntry RecordPayment(Guid companyId, Guid customerId, Guid invoiceId, decimal amount, string? reference, DateTimeOffset? transactionDate = null)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer id is required.", nameof(customerId));
        if (invoiceId == Guid.Empty)
            throw new ArgumentException("Invoice id is required.", nameof(invoiceId));
        if (amount <= 0)
            throw new ArgumentException("A payment amount must be positive.", nameof(amount));

        var description = string.IsNullOrWhiteSpace(reference)
            ? $"Payment against invoice {invoiceId}"
            : $"Payment against invoice {invoiceId} ({reference.Trim()})";

        return new ArLedgerEntry
        {
            CompanyId = companyId,
            CustomerId = customerId,
            InvoiceId = invoiceId,
            Amount = -amount,
            Description = description,
            TransactionDate = transactionDate ?? DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Records a credit note issued against a specific invoice as a negative
    /// ledger entry — distinguished from RecordPayment only by Description text,
    /// same "same factory shape, different Reason text" restraint used for
    /// Inventory adjustments (manual/GoodsReceipt/CycleCount all reuse
    /// InventoryLedgerEntry.RecordAdjustment). Created by CreditNoteIssuedConsumer
    /// reacting to Sales' CreditNoteIssued event, the same shape as
    /// InvoiceIssuedConsumer creating a RecordInvoiceCharge entry.
    /// </summary>
    public static ArLedgerEntry RecordCreditNote(Guid companyId, Guid customerId, Guid invoiceId, Guid creditNoteId, decimal amount)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer id is required.", nameof(customerId));
        if (invoiceId == Guid.Empty)
            throw new ArgumentException("Invoice id is required.", nameof(invoiceId));
        if (amount <= 0)
            throw new ArgumentException("A credit note amount must be positive.", nameof(amount));

        return new ArLedgerEntry
        {
            CompanyId = companyId,
            CustomerId = customerId,
            InvoiceId = invoiceId,
            Amount = -amount,
            Description = $"Credit note {creditNoteId} against invoice {invoiceId}",
            TransactionDate = DateTimeOffset.UtcNow,
        };
    }
}
