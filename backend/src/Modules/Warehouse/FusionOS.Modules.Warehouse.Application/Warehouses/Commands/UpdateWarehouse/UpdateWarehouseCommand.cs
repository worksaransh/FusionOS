using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Warehouses.Commands.UpdateWarehouse;

/// <summary>
/// Requires the new "warehouse.warehouse.update" permission (not yet in
/// PermissionCatalog.cs — must be added centrally). Code is intentionally not
/// editable here — see Warehouse.UpdateDetails.
/// </summary>
public sealed record UpdateWarehouseCommand(Guid CompanyId, Guid Id, Guid? BranchId, string Name, string? Address)
    : ICommand<WarehouseDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.warehouse.update" };
    public string EntityType => nameof(Domain.Warehouses.Warehouse);
    public Guid EntityId => Id;
    public string Action => "Updated";
}
