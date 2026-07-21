using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Racks.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Racks.Commands.CreateRack;

public sealed record CreateRackCommand(Guid CompanyId, Guid ZoneId, string Name, string Code)
    : ICommand<RackDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.rack.create" };
    public string EntityType => nameof(Domain.Racks.Rack);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
