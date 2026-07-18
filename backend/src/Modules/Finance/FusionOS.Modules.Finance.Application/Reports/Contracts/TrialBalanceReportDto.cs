namespace FusionOS.Modules.Finance.Application.Reports.Contracts;

/// <summary>
/// One account's line on the trial balance: the total debits and total credits
/// posted to it as of the report date, plus the net (TotalDebit - TotalCredit;
/// positive = net debit, negative = net credit). Account code/name are resolved
/// from the Account aggregate the same way GetBudgetVsActualQueryHandler resolves
/// them for its own rows.
/// </summary>
public sealed record TrialBalanceLineDto(
    Guid AccountId,
    string AccountCode,
    string AccountName,
    decimal TotalDebit,
    decimal TotalCredit,
    decimal NetBalance);

/// <summary>
/// Trial balance as of a date: every account with any Posted activity, its debit
/// and credit totals side by side, and the grand totals. IsBalanced restates the
/// double-entry invariant at the report level — for a ledger built only from
/// balanced JournalEntries it should always be true, and surfacing it lets a
/// reader catch any drift immediately rather than eyeballing two columns.
/// </summary>
public sealed record TrialBalanceReportDto(
    DateTimeOffset AsOfDate,
    IReadOnlyList<TrialBalanceLineDto> Lines,
    decimal TotalDebit,
    decimal TotalCredit,
    bool IsBalanced);
