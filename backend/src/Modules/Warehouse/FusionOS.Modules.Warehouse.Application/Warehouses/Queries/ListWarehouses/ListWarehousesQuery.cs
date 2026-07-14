using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Warehouses.Queries.ListWarehouses;

public sealed record ListWarehousesQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25) : IQuery<PagedResult<WarehouseDto>>;
