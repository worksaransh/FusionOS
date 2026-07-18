using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Warehouses.Queries.GetWarehouseById;

/// <summary>
/// Wires the dead GetById stub in WarehousesController (same documented gap as
/// CompaniesController). Tenant-scoped via the CompanyId property, which
/// TenantIsolationBehavior enforces against the caller's own company; the
/// handler additionally checks the loaded Warehouse's CompanyId to guard
/// against a cross-tenant lookup by guessed id. Read-gated with the existing
/// "warehouse.warehouse.read" permission — no new permission code needed here.
/// </summary>
public sealed record GetWarehouseByIdQuery(Guid CompanyId, Guid Id)
    : IQuery<WarehouseDto?>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "warehouse.warehouse.read" };
}
