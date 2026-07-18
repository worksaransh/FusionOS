using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.Reports.Contracts;

namespace FusionOS.Modules.Procurement.Application.Reports.Queries.GetPoStatusSummaryReport;

/// <summary>Read-gated on the same permission as every other purchase-order read (Phase M6, 2026-07-15).</summary>
public sealed record GetPoStatusSummaryReportQuery(Guid CompanyId) : IQuery<PoStatusSummaryReportDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "procurement.purchase-order.read" };
}
