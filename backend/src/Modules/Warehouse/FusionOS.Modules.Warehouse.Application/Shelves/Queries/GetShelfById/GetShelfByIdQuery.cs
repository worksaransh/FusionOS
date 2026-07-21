using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Shelves.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Shelves.Queries.GetShelfById;

public sealed record GetShelfByIdQuery(Guid CompanyId, Guid Id)
    : IQuery<ShelfDto?>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "warehouse.shelf.read" };
}
