using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Bins.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Bins.Commands.AssignBinShelf;

/// <summary>
/// Sets or clears (pass ShelfId = null) a bin's optional Shelf refinement.
/// Reuses "warehouse.bin.update" — this only refines an existing bin's
/// location, not a new permission-worthy action (no new permission code
/// needed here, matching the task's guidance).
/// </summary>
public sealed record AssignBinShelfCommand(Guid CompanyId, Guid BinId, Guid? ShelfId)
    : ICommand<BinDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.bin.update" };
    public string EntityType => nameof(Domain.Bins.Bin);
    public Guid EntityId => BinId;
    public string Action => "ShelfAssigned";
}
