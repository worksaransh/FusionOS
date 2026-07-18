namespace FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;

public sealed record BankStatementLineDto(
    Guid Id,
    Guid BankAccountId,
    DateTimeOffset TransactionDate,
    decimal Amount,
    string Description,
    bool IsReconciled,
    DateTimeOffset? ReconciledAt,
    Guid? MatchedJournalEntryId);

/// <summary>Backs GetReconciliationSummaryQuery — a single bank account's reconciliation rollup, not a paged list.</summary>
public sealed record ReconciliationSummaryDto(
    Guid BankAccountId,
    int TotalLines,
    int ReconciledCount,
    int UnreconciledCount,
    decimal UnreconciledTotalAmount);

/// <summary>
/// One candidate posted JournalEntry that SuggestMatchesForStatementLineQuery
/// proposes as a possible match for a bank statement line. Amount is the entry's
/// balanced magnitude (TotalDebit == TotalCredit). This is only a suggestion — the
/// user still confirms by calling ReconcileStatementLineCommand with the chosen
/// JournalEntryId, keeping BankStatementLine's "no auto-matching, a human always
/// confirms" scope line honest.
/// </summary>
public sealed record JournalEntryMatchCandidateDto(
    Guid JournalEntryId,
    DateTimeOffset EntryDate,
    string? Reference,
    decimal Amount);
