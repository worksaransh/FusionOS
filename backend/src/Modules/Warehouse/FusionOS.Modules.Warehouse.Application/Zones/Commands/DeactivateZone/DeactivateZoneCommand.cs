using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Zones.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Zones.Commands.DeactivateZone;

/// <summary>
/// Soft-deactivation only — never deletes the row (08_API_STANDARDS.md /
/// 04_DATABASE_GUIDELINES.md). Requires the new "warehouse.zone.deactivate"
/// permission (not yet in PermissionCatalog.cs — must be added centrally).
/// </summary>
public sealed record DeactivateZoneCommand(Guid CompanyId, Guid Id)
    : ICommand<ZoneDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.zone.deactivate" };
    public string EntityType => nameof(Domain.Zones.Zone);
    public Guid EntityId => Id;
    public string Action => "Deactivated";
}
