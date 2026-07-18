using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Zones.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Zones.Queries.ListZones;

/// <summary>Read-gated (2026-07-14 sprint audit) — see ListAccountsQuery for rationale.</summary>
public sealed record ListZonesQuery(Guid CompanyId, Guid WarehouseId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<ZoneDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "warehouse.zone.read" };
}
