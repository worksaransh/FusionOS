using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.CycleCounts.Contracts;

namespace FusionOS.Modules.Warehouse.Application.CycleCounts.Queries.GetCycleCountById;

public sealed record GetCycleCountByIdQuery(Guid CompanyId, Guid Id)
    : IQuery<CycleCountDto?>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "warehouse.cycle-count.read" };
}
