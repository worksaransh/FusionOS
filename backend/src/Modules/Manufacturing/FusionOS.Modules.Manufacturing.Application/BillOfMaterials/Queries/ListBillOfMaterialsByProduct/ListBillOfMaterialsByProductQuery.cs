using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Queries.ListBillOfMaterialsByProduct;

/// <summary>
/// Alternative BOM (Phase 6, 2026-07-22). Every active, current-version bill of materials
/// for a product — the primary one plus any alternatives — so a user (or a Work Order
/// picker) can see them side by side.
/// </summary>
public sealed record ListBillOfMaterialsByProductQuery(Guid CompanyId, Guid ProductId)
    : IQuery<IReadOnlyList<BillOfMaterialsDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "manufacturing.bill-of-materials.read" };
}
