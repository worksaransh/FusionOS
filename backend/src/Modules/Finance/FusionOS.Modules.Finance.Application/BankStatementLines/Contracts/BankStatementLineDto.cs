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

/// <summary>
/// One row handed to ImportBankStatementLinesCommand by
/// BankStatementLinesController after it has already parsed the uploaded
/// file (see the controller's own doc comment for the supported CSV
/// format). RowNumber is the 1-based data-row position in the original
/// file (header excluded) — carried through so a row error reported back
/// by the handler can be matched to the exact line the caller uploaded,
/// the same way ImportBankStatementLineRowErrorDto.RowNumber is used.
/// </summary>
public sealed record ImportBankStatementLineInput(
    int RowNumber,
    DateTimeOffset TransactionDate,
    decimal Amount,
    string Description);

/// <summary>One row that could not be imported, either because the controller couldn't parse it or because the handler rejected it (e.g. a zero amount).</summary>
public sealed record ImportBankStatementLineRowErrorDto(int RowNumber, string Message);

/// <summary>
/// Summary returned by both ImportBankStatementLinesCommandHandler and the
/// import endpoint itself. RowsParsed always means the total number of data
/// rows the caller attempted to import (both well-formed and malformed) —
/// the controller widens the handler's own RowsParsed (which only counts
/// well-formed rows it was handed) to include its own parse failures before
/// returning this DTO to the client, using a `with` expression rather than a
/// second constructor.
/// </summary>
public sealed record BankStatementLineImportResultDto(
    int RowsParsed,
    int RowsImported,
    int RowsSkippedAsDuplicate,
    IReadOnlyList<ImportBankStatementLineRowErrorDto> Errors);
