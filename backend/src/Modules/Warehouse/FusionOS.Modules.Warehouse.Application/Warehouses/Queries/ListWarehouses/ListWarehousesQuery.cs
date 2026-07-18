using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Warehouses.Queries.ListWarehouses;

/// <summary>Read-gated (2026-07-14 sprint audit) — see ListAccountsQuery for rationale.</summary>
public sealed record ListWarehousesQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<WarehouseDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "warehouse.warehouse.read" };
}
