using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Shelves.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Shelves.Commands.DeactivateShelf;

/// <summary>Soft-deactivation only — never deletes the row (08_API_STANDARDS.md / 04_DATABASE_GUIDELINES.md).</summary>
public sealed record DeactivateShelfCommand(Guid CompanyId, Guid Id)
    : ICommand<ShelfDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.shelf.deactivate" };
    public string EntityType => nameof(Domain.Shelves.Shelf);
    public Guid EntityId => Id;
    public string Action => "Deactivated";
}
