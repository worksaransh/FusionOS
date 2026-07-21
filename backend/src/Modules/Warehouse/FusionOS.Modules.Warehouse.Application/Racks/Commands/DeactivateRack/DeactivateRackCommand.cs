using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Racks.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Racks.Commands.DeactivateRack;

/// <summary>Soft-deactivation only — never deletes the row (08_API_STANDARDS.md / 04_DATABASE_GUIDELINES.md).</summary>
public sealed record DeactivateRackCommand(Guid CompanyId, Guid Id)
    : ICommand<RackDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.rack.deactivate" };
    public string EntityType => nameof(Domain.Racks.Rack);
    public Guid EntityId => Id;
    public string Action => "Deactivated";
}
