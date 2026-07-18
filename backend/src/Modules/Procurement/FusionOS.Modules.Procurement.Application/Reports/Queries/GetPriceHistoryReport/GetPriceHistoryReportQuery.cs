using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.Reports.Contracts;

namespace FusionOS.Modules.Procurement.Application.Reports.Queries.GetPriceHistoryReport;

/// <summary>Read-gated on its own report permission, same convention as the other Phase M6 canned reports.</summary>
public sealed record GetPriceHistoryReportQuery(Guid CompanyId, Guid ProductId)
    : IQuery<IReadOnlyList<PriceHistoryLineDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "procurement.price-history.read" };
}
