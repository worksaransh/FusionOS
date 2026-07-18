using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.PickLists.Contracts;

namespace FusionOS.Modules.Warehouse.Application.PickLists.Queries.GetPickListById;

public sealed record GetPickListByIdQuery(Guid CompanyId, Guid Id)
    : IQuery<PickListDto?>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "warehouse.pick-list.read" };
}
