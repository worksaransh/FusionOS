namespace FusionOS.Modules.Finance.Application.JournalEntries.Contracts;

public interface IJournalEntryRepository
{
    Task<Domain.JournalEntries.JournalEntry?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.JournalEntries.JournalEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.JournalEntries.JournalEntry>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Added for M8f (budgeting) — GetBudgetVsActualQueryHandler's "actual"
    /// side. Sums (Debit - Credit) across every line for accountId, from
    /// every JournalEntry for this company whose Status is Posted (Draft
    /// entries never affect the ledger, see JournalEntry.cs) and whose
    /// EntryDate falls within [dateFrom, dateTo] inclusive. Returns a single
    /// net signed amount, same "one repository-owned sum" shape as
    /// IApLedgerRepository.SumAmountAsync/IArLedgerRepository.SumAmountAsync
    /// — this deliberately does not flip sign per Account.AccountType's
    /// normal balance side (e.g. revenue is credit-normal); a caller
    /// comparing this to a budgeted amount interprets the sign itself.
    /// When <paramref name="costCenterId"/> is supplied the sum is further
    /// restricted to lines carrying exactly that CostCenterId (now that
    /// JournalEntryLine carries one); when null, the behaviour is unchanged —
    /// account-level across all cost centers.
    /// </summary>
    Task<decimal> SumPostedAmountByAccountAsync(Guid companyId, Guid accountId, DateTimeOffset dateFrom, DateTimeOffset dateTo, Guid? costCenterId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Backs SuggestMatchesForStatementLineQuery (bank reconciliation). Returns every
    /// Posted JournalEntry for this company whose balanced magnitude (TotalDebit ==
    /// TotalCredit) equals <paramref name="amountMagnitude"/> and whose EntryDate falls
    /// within [dateFrom, dateTo] inclusive. Draft entries are never returned (same
    /// Posted-only rule as SumPostedAmountByAccountAsync). This only proposes candidates
    /// by amount+date proximity; it does not itself decide a match — the user confirms.
    /// </summary>
    Task<IReadOnlyList<Domain.JournalEntries.JournalEntry>> FindPostedByAmountWithinDateRangeAsync(Guid companyId, decimal amountMagnitude, DateTimeOffset dateFrom, DateTimeOffset dateTo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Backs GetTrialBalanceQuery. Sums every Posted JournalEntryLine (Draft entries
    /// never affect the ledger) whose parent entry's EntryDate is on or before
    /// <paramref name="asOfDate"/>, grouped by AccountId, returning per-account total
    /// debit and total credit. Sign is not flipped per account normal-balance side —
    /// the caller derives net debit-minus-credit itself, same convention as
    /// SumPostedAmountByAccountAsync.
    /// </summary>
    Task<IReadOnlyList<(Guid AccountId, decimal TotalDebit, decimal TotalCredit)>> GetPostedBalancesByAccountAsOfAsync(Guid companyId, DateTimeOffset asOfDate, CancellationToken cancellationToken = default);
}
