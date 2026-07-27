namespace FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;

/// <summary>
/// Mirrors IApLedgerRepository's "append, list by parent, count" shape, plus
/// GetByIdAsync — needed here because, unlike ApLedgerEntry's append-only
/// ledger, Reconcile/Unreconcile must load and mutate an existing row — and
/// GetReconciliationSummaryAsync, backing GetReconciliationSummaryQueryHandler's
/// single-bank-account rollup (total/reconciled/unreconciled counts plus the
/// unreconciled total amount).
/// </summary>
public interface IBankStatementLineRepository
{
    Task<Domain.BankStatementLines.BankStatementLine?> GetByIdAsync(Guid companyId, Guid statementLineId, CancellationToken cancellationToken = default);

    Task AddAsync(Domain.BankStatementLines.BankStatementLine line, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Domain.BankStatementLines.BankStatementLine>> ListByBankAccountAsync(Guid companyId, Guid bankAccountId, bool? isReconciled, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountByBankAccountAsync(Guid companyId, Guid bankAccountId, bool? isReconciled, CancellationToken cancellationToken = default);

    Task<(int TotalLines, int ReconciledCount, int UnreconciledCount, decimal UnreconciledTotalAmount)> GetReconciliationSummaryAsync(Guid companyId, Guid bankAccountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Every distinct JournalEntry id already picked as a match by some reconciled
    /// statement line for this company. SuggestMatchesForStatementLineQuery uses this to
    /// exclude entries that are already matched, so a JournalEntry is never suggested to
    /// two different statement lines. "Unreconciled candidate" == not in this set.
    /// </summary>
    Task<IReadOnlyList<Guid>> GetMatchedJournalEntryIdsAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Used by ImportBankStatementLinesCommandHandler to skip a row that's
    /// already been imported for this bank account — "already imported" is
    /// defined as an exact match on TransactionDate + Amount + Description
    /// (post-trim), same fields BankStatementLine.Create itself validates.
    /// </summary>
    Task<bool> ExistsDuplicateAsync(Guid companyId, Guid bankAccountId, DateTimeOffset transactionDate, decimal amount, string description, CancellationToken cancellationToken = default);
}
