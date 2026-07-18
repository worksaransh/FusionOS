using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Commissions.Contracts;

namespace FusionOS.Modules.Sales.Application.Commissions.Queries.GetSalesCommissionSummaryReport;

/// <summary>Read-gated on a dedicated report permission, same convention as the Phase M6 canned reports (AR aging, stock valuation, PO status summary).</summary>
public sealed record GetSalesCommissionSummaryReportQuery(Guid CompanyId)
    : IQuery<IReadOnlyList<SalesCommissionSummaryLineDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "sales.commission-report.read" };
}
