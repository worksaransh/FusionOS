using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Queries.GetVersionHistory;

/// <summary>
/// BOM Versioning (Phase 6, 2026-07-22). Every version in <see cref="BillOfMaterialsId"/>'s
/// version chain (including superseded/non-current ones), newest version first — lets a
/// user see how a bill of materials evolved over time.
/// </summary>
public sealed record GetVersionHistoryQuery(Guid CompanyId, Guid BillOfMaterialsId)
    : IQuery<IReadOnlyList<BillOfMaterialsDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "manufacturing.bill-of-materials.read" };
}
