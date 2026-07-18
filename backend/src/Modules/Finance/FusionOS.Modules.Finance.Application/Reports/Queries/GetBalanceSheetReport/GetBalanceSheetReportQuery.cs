using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Reports.Contracts;

namespace FusionOS.Modules.Finance.Application.Reports.Queries.GetBalanceSheetReport;

/// <summary>Read-gated on the same permission as the trial balance and journal-entry list — a canned report over the ledger is still just a read.</summary>
public sealed record GetBalanceSheetReportQuery(Guid CompanyId, DateTimeOffset AsOfDate)
    : IQuery<BalanceSheetReportDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.journal-entry.read" };
}
