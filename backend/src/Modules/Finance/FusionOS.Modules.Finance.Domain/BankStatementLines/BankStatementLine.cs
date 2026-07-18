using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.BankStatementLines;

/// <summary>
/// M8d — Finance depth: bank reconciliation. One line from a bank
/// statement, entered by hand.
///
/// <b>Scope decision (Phase M8d, 2026-07-17), documented here the same way
/// ApLedgerEntry's own class doc comment documents its scope-out:</b> there
/// is no bank-feed/file-import integration in this slice (no OFX/CSV/
/// Plaid-style connector) — every line is created one at a time via
/// RecordStatementLineCommand, same manual-entry-first discipline AP started
/// with. A real bank-feed integration is a distinct, separately-scoped
/// follow-up, not something to fold into "build the reconciliation master
/// data." Likewise, matching a line to an existing JournalEntry
/// (<see cref="MatchedJournalEntryId"/>) is always a manual pick by the user
/// via ReconcileStatementLineCommand — there is no auto-matching algorithm
/// here (e.g. matching by amount/date proximity), which would be a
/// substantial separate feature of its own, also out of scope. This aggregate
/// does not verify MatchedJournalEntryId actually exists — same
/// domain-shape-only / handler-checks-cross-aggregate-existence split
/// BankAccount.Create uses for LinkedAccountId, and the same "opaque
/// reference, not validated here" precedent ApLedgerEntry.SupplierId/
/// PurchaseOrderId set for Procurement references.
///
/// Unlike CostCenter (soft-deactivate, one-way) and ApLedgerEntry
/// (append-only, never mutated), reconciliation is inherently a toggle-able
/// state — so, uniquely among this phase's aggregates, this one exposes both
/// <see cref="Reconcile"/> and its inverse <see cref="Unreconcile"/>. No
/// history/audit trail beyond what IAuditableCommand already records at the
/// command level is kept for toggling back and forth.
/// </summary>
public sealed class BankStatementLine : TenantAggregateRoot
{
    public Guid BankAccountId { get; private set; }
    public DateTimeOffset TransactionDate { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = default!;
    public bool IsReconciled { get; private set; }
    public DateTimeOffset? ReconciledAt { get; private set; }
    public Guid? MatchedJournalEntryId { get; private set; }

    private BankStatementLine() { }

    /// <summary>
    /// Amount can be positive (a deposit) or negative (a withdrawal) — the
    /// bank statement's own sign, not normalized the way ApLedgerEntry
    /// normalizes charges/payments to a fixed sign convention, since a
    /// statement line has no separate "kind" the way a charge/payment does.
    /// </summary>
    public static BankStatementLine Create(Guid companyId, Guid bankAccountId, DateTimeOffset transactionDate, decimal amount, string description)
    {
        if (bankAccountId == Guid.Empty)
            throw new ArgumentException("Bank account id is required.", nameof(bankAccountId));
        if (amount == 0)
            throw new ArgumentException("A statement line amount cannot be zero.", nameof(amount));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A statement line description is required.", nameof(description));

        return new BankStatementLine
        {
            CompanyId = companyId,
            BankAccountId = bankAccountId,
            TransactionDate = transactionDate,
            Amount = amount,
            Description = description.Trim(),
            IsReconciled = false,
        };
    }

    /// <summary>
    /// Marks this line reconciled, optionally recording which JournalEntry
    /// it corresponds to. MatchedJournalEntryId's existence is a
    /// handler-level concern, not enforced here (see class doc comment).
    /// </summary>
    public void Reconcile(Guid? matchedJournalEntryId)
    {
        IsReconciled = true;
        ReconciledAt = DateTimeOffset.UtcNow;
        MatchedJournalEntryId = matchedJournalEntryId;
    }

    /// <summary>
    /// Reverses Reconcile — resets all three reconciliation-state fields
    /// back to their pre-Reconcile defaults. A toggle, not a soft-delete:
    /// the line itself is never removed.
    /// </summary>
    public void Unreconcile()
    {
        IsReconciled = false;
        ReconciledAt = null;
        MatchedJournalEntryId = null;
    }
}
