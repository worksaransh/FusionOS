using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Zones.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Zones.Commands.CreateZone;

public sealed record CreateZoneCommand(Guid CompanyId, Guid WarehouseId, string Name, string Code)
    : ICommand<ZoneDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.zone.create" };
    public string EntityType => nameof(Domain.Zones.Zone);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
