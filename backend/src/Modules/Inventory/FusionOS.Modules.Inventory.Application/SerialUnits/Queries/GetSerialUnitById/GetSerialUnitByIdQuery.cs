using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.SerialUnits.Contracts;

namespace FusionOS.Modules.Inventory.Application.SerialUnits.Queries.GetSerialUnitById;

public sealed record GetSerialUnitByIdQuery(Guid CompanyId, Guid Id)
    : IQuery<SerialUnitDto?>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "inventory.serial.read" };
}
