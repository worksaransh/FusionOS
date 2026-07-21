using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Shelves.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Shelves.Queries.ListShelves;

public sealed record ListShelvesQuery(Guid CompanyId, Guid RackId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<ShelfDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "warehouse.shelf.read" };
}
