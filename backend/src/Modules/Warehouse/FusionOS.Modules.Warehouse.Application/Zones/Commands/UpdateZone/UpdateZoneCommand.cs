using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Zones.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Zones.Commands.UpdateZone;

/// <summary>
/// Requires the new "warehouse.zone.update" permission (not yet in
/// PermissionCatalog.cs — must be added centrally). WarehouseId and Code are
/// intentionally not editable here — see Zone.UpdateDetails.
/// </summary>
public sealed record UpdateZoneCommand(Guid CompanyId, Guid Id, string Name)
    : ICommand<ZoneDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.zone.update" };
    public string EntityType => nameof(Domain.Zones.Zone);
    public Guid EntityId => Id;
    public string Action => "Updated";
}
