using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Racks.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Racks.Queries.ListRacks;

public sealed record ListRacksQuery(Guid CompanyId, Guid ZoneId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<RackDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "warehouse.rack.read" };
}
