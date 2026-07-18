using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.CycleCounts.Contracts;

namespace FusionOS.Modules.Warehouse.Application.CycleCounts.Commands.StartCycleCount;

/// <summary>
/// Opens a new cycle count for one product at one bin. SystemQuantitySnapshot
/// is supplied by the caller (read from Inventory's GET /stock/on-hand just
/// before calling this) — this module has no cross-module read of its own,
/// same "opaque reference, caller supplies the data" convention already used
/// throughout (see CycleCount.cs's doc comment). StartedBy always comes from
/// the authenticated caller (ICurrentUserContext), never a client-supplied
/// value, same reasoning as ApprovalRequest.RequestedBy.
/// </summary>
public sealed record StartCycleCountCommand(Guid CompanyId, Guid WarehouseId, Guid ZoneId, Guid BinId, Guid ProductId, decimal SystemQuantitySnapshot)
    : ICommand<CycleCountDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.cycle-count.create" };
    public string EntityType => nameof(Domain.CycleCounts.CycleCount);
    public Guid EntityId { get; init; }
    public string Action => "Started";
}
