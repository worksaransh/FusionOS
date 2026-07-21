using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Shelves.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Shelves.Commands.UpdateShelf;

/// <summary>RackId and Code are intentionally not editable here — see Shelf.UpdateDetails.</summary>
public sealed record UpdateShelfCommand(Guid CompanyId, Guid Id, string Name)
    : ICommand<ShelfDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.shelf.update" };
    public string EntityType => nameof(Domain.Shelves.Shelf);
    public Guid EntityId => Id;
    public string Action => "Updated";
}
