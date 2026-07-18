using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Queries.ListBillOfMaterials;

public sealed record ListBillOfMaterialsQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<BillOfMaterialsDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "manufacturing.bill-of-materials.read" };
}
