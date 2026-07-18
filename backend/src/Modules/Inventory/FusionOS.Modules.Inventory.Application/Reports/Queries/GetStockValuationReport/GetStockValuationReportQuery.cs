using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Reports.Contracts;

namespace FusionOS.Modules.Inventory.Application.Reports.Queries.GetStockValuationReport;

/// <summary>Read-gated on the same permission as every other stock read (Phase M6, 2026-07-15).</summary>
public sealed record GetStockValuationReportQuery(Guid CompanyId) : IQuery<StockValuationReportDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "inventory.stock.read" };
}
