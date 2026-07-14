using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Zones.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Zones.Queries.ListZones;

public sealed record ListZonesQuery(Guid CompanyId, Guid WarehouseId, int Page = 1, int PageSize = 25) : IQuery<PagedResult<ZoneDto>>;
