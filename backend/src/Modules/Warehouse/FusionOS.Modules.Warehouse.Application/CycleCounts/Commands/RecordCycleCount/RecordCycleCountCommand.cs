using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.CycleCounts.Contracts;

namespace FusionOS.Modules.Warehouse.Application.CycleCounts.Commands.RecordCycleCount;

/// <summary>
/// Submits the physically-counted quantity for a pending cycle count. If a
/// variance results, CycleCount.RecordCount raises CycleCountVarianceRecorded,
/// which the outbox relays to Inventory's consumer to post a ledger
/// adjustment — see CycleCount.cs's doc comment for the full chain.
/// </summary>
public sealed record RecordCycleCountCommand(Guid CompanyId, Guid Id, decimal CountedQuantity)
    : ICommand<CycleCountDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.cycle-count.record" };
    public string EntityType => nameof(Domain.CycleCounts.CycleCount);
    public Guid EntityId => Id;
    public string Action => "Recorded";
}
