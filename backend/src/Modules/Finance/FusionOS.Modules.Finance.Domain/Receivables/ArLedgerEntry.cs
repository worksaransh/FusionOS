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
/// Only "charge" entries (positive Amount, one per issued invoice) exist
/// today. Recording customer payments (negative Amount, reducing the
/// balance) is a real, distinct follow-up slice — not built here — since it
/// needs its own command/endpoint and does not arrive via any existing
/// integration event.
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
}
