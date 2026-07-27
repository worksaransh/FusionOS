using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Reports.Contracts;

namespace FusionOS.Modules.Finance.Application.Reports.Queries.GetCashFlowStatementReport;

/// <summary>Read-gated on the same permission as the trial balance/P&amp;L/balance sheet/journal-entry list — a canned report over the ledger is still just a read.</summary>
public sealed record GetCashFlowStatementReportQuery(Guid CompanyId, DateTimeOffset PeriodStart, DateTimeOffset PeriodEnd)
    : IQuery<CashFlowStatementReportDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.journal-entry.read" };
}
