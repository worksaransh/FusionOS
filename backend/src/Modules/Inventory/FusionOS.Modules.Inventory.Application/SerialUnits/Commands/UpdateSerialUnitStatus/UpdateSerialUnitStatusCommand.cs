using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.SerialUnits.Contracts;
using FusionOS.Modules.Inventory.Domain.SerialUnits;

namespace FusionOS.Modules.Inventory.Application.SerialUnits.Commands.UpdateSerialUnitStatus;

/// <summary>
/// Drives one of SerialUnit's Mark* transitions. NewStatus must be Reserved,
/// Sold, Returned, or Defective — InStock is only ever set at registration
/// (Create), there is no "revert to InStock" transition, matching the
/// aggregate's own state machine (see SerialUnit.cs doc comment).
/// </summary>
public sealed record UpdateSerialUnitStatusCommand(Guid CompanyId, Guid SerialUnitId, SerialUnitStatus NewStatus)
    : ICommand<SerialUnitDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.serial.update" };
    public string EntityType => nameof(Domain.SerialUnits.SerialUnit);
    public Guid EntityId => SerialUnitId;
    public string Action => "StatusChanged";
}
