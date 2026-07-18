using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Reports.Contracts;

namespace FusionOS.Modules.Inventory.Application.Reports.Queries.GetInventoryValuationReport;

/// <summary>Weighted-average-cost valuation report (M9 remaining — Inventory costing, 2026-07-16).</summary>
public sealed record GetInventoryValuationReportQuery(Guid CompanyId) : IQuery<InventoryValuationReportDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "inventory.costing.read" };
}
