using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.SerialUnits.Contracts;
using FusionOS.Modules.Inventory.Domain.SerialUnits;

namespace FusionOS.Modules.Inventory.Application.SerialUnits.Queries.ListSerialUnitsByProduct;

public sealed record ListSerialUnitsByProductQuery(Guid CompanyId, Guid ProductId, SerialUnitStatus? Status = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<SerialUnitDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "inventory.serial.read" };
}
