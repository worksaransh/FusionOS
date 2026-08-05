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

    /// <summary>
    /// Backs GetProfitAndLossReportQuery (Phase 2 closeout, 2026-07-18). Same shape as
    /// GetPostedBalancesByAccountAsOfAsync but scoped to a period [dateFrom, dateTo]
    /// inclusive rather than "since inception through a date" — a P&amp;L reports Revenue/
    /// Expense activity for a period, not a cumulative balance the way a Balance Sheet's
    /// Asset/Liability/Equity snapshot does.
    /// </summary>
    Task<IReadOnlyList<(Guid AccountId, decimal TotalDebit, decimal TotalCredit)>> GetPostedBalancesByAccountInRangeAsync(Guid companyId, DateTimeOffset dateFrom, DateTimeOffset dateTo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Backs CloseAccountingPeriodCommandHandler's close-time guard (Phase 10/11 hardening,
    /// 2026-07-27): a period must not be closeable while a Draft entry still sits inside its
    /// [dateFrom, dateTo] range, because PostJournalEntryCommandHandler's own closed-period
    /// check would then make that Draft permanently unpostable with no recovery path short of
    /// reopening the whole period. Only Draft entries matter here — Posted entries are exactly
    /// what closing a period is supposed to lock in.
    /// </summary>
    Task<bool> ExistsDraftEntryInDateRangeAsync(Guid companyId, DateTimeOffset dateFrom, DateTimeOffset dateTo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Backs RevalueForeignCurrencyBalancesCommandHandler. Sums every Posted line tagged with
    /// <paramref name="currencyCode"/> (see JournalEntryLine.ForeignCurrencyCode/ForeignAmount),
    /// grouped by AccountId — NetForeignAmount is the signed (debit positive, credit negative)
    /// sum of ForeignAmount, NetBaseAmount is the signed sum of (Debit - Credit) in the
    /// company's base currency at the rate each line was originally posted at. The difference
    /// between NetForeignAmount priced at a new rate and NetBaseAmount is the realized/unrealized
    /// FX gain or loss the revaluation command posts. Lines with no ForeignCurrencyCode (the
    /// vast majority — ordinary base-currency transactions) are excluded entirely, not treated
    /// as zero-exposure.
    /// </summary>
    Task<IReadOnlyList<(Guid AccountId, decimal NetForeignAmount, decimal NetBaseAmount)>> GetOpenForeignCurrencyBalancesByAccountAsync(Guid companyId, string currencyCode, CancellationToken cancellationToken = default);
}
