using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Warehouses.Commands.DeactivateWarehouse;

/// <summary>
/// Soft-deactivation only — never deletes the row (08_API_STANDARDS.md /
/// 04_DATABASE_GUIDELINES.md). Requires the new "warehouse.warehouse.deactivate"
/// permission (not yet in PermissionCatalog.cs — must be added centrally).
/// </summary>
public sealed record DeactivateWarehouseCommand(Guid CompanyId, Guid Id)
    : ICommand<WarehouseDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.warehouse.deactivate" };
    public string EntityType => nameof(Domain.Warehouses.Warehouse);
    public Guid EntityId => Id;
    public string Action => "Deactivated";
}
