using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.Reports.Contracts;

namespace FusionOS.Modules.Procurement.Application.Reports.Queries.GetSupplierScorecardReport;

/// <summary>Read-gated on its own report permission (pre-added to PermissionCatalog alongside the RFQ pass), same convention as the other Phase M6 canned reports.</summary>
public sealed record GetSupplierScorecardReportQuery(Guid CompanyId)
    : IQuery<IReadOnlyList<SupplierScorecardLineDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "procurement.supplier-scorecard.read" };
}
