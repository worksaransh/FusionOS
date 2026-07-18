using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Reports.Contracts;

namespace FusionOS.Modules.Finance.Application.Reports.Queries.GetTrialBalance;

/// <summary>
/// The trial balance — the one piece of core General Ledger reporting the ledger
/// was missing: every account's net position from Posted JournalEntry activity as
/// of a date. Read-gated on the same finance.journal-entry.read permission as the
/// journal-entry list — a canned report over the ledger is still just a read.
/// </summary>
public sealed record GetTrialBalanceQuery(Guid CompanyId, DateTimeOffset AsOfDate)
    : IQuery<TrialBalanceReportDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.journal-entry.read" };
}
