using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.SerialUnits.Contracts;

namespace FusionOS.Modules.Inventory.Application.SerialUnits.Commands.RegisterSerialUnit;

public sealed record RegisterSerialUnitCommand(Guid CompanyId, Guid ProductId, string SerialNumber)
    : ICommand<SerialUnitDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.serial.create" };
    public string EntityType => nameof(Domain.SerialUnits.SerialUnit);
    public Guid EntityId { get; init; }
    public string Action => "Registered";
}
