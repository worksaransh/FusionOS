using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.CycleCounts.Contracts;

namespace FusionOS.Modules.Warehouse.Application.CycleCounts.Queries.ListCycleCounts;

public sealed record ListCycleCountsQuery(Guid CompanyId, Guid WarehouseId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<CycleCountDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "warehouse.cycle-count.read" };
}
