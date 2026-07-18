using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;

namespace FusionOS.Modules.Finance.Application.BankStatementLines.Queries.SuggestMatchesForStatementLine;

/// <summary>
/// The simple (deliberately not a bank-feed-import) half of bank auto-matching:
/// for one unreconciled statement line, suggest posted JournalEntries with the same
/// amount within a small date window (+/-<see cref="DateToleranceDays"/>, default 3),
/// as candidates the user then confirms via ReconcileStatementLineCommand. This keeps
/// BankStatementLine's "no auto-matching algorithm — a human always confirms the
/// match" scope line honest: this query only proposes, it never reconciles anything
/// itself. Gated by the same finance.bank-statement-line.read permission as the other
/// reconciliation reads. A full bank-feed/file-import connector remains out of scope.
/// </summary>
public sealed record SuggestMatchesForStatementLineQuery(Guid CompanyId, Guid StatementLineId, int DateToleranceDays = 3)
    : IQuery<IReadOnlyList<JournalEntryMatchCandidateDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.bank-statement-line.read" };
}
