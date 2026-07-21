using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Shelves.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Shelves.Commands.CreateShelf;

public sealed record CreateShelfCommand(Guid CompanyId, Guid RackId, string Name, string Code)
    : ICommand<ShelfDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.shelf.create" };
    public string EntityType => nameof(Domain.Shelves.Shelf);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
