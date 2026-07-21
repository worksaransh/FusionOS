using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Racks.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Racks.Queries.GetRackById;

public sealed record GetRackByIdQuery(Guid CompanyId, Guid Id)
    : IQuery<RackDto?>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "warehouse.rack.read" };
}
