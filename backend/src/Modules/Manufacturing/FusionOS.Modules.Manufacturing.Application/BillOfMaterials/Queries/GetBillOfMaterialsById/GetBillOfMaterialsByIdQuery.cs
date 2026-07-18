using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Queries.GetBillOfMaterialsById;

public sealed record GetBillOfMaterialsByIdQuery(Guid CompanyId, Guid BillOfMaterialsId)
    : IQuery<BillOfMaterialsDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "manufacturing.bill-of-materials.read" };
}
