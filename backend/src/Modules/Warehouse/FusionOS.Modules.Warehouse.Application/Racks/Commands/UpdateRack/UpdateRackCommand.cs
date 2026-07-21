using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Racks.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Racks.Commands.UpdateRack;

/// <summary>ZoneId and Code are intentionally not editable here — see Rack.UpdateDetails.</summary>
public sealed record UpdateRackCommand(Guid CompanyId, Guid Id, string Name)
    : ICommand<RackDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.rack.update" };
    public string EntityType => nameof(Domain.Racks.Rack);
    public Guid EntityId => Id;
    public string Action => "Updated";
}
