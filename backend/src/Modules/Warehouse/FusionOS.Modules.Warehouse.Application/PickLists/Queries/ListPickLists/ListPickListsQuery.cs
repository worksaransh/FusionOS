using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.PickLists.Contracts;

namespace FusionOS.Modules.Warehouse.Application.PickLists.Queries.ListPickLists;

public sealed record ListPickListsQuery(Guid CompanyId, Guid WarehouseId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<PickListDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "warehouse.pick-list.read" };
}
