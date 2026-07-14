using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Warehouses.Commands.CreateWarehouse;

public sealed record CreateWarehouseCommand(Guid CompanyId, Guid? BranchId, string Name, string Code, string? Address)
    : ICommand<WarehouseDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.warehouse.create" };
    public string EntityType => nameof(Domain.Warehouses.Warehouse);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
