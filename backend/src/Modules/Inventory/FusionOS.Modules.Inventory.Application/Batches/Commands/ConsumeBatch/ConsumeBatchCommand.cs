using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Batches.Contracts;

namespace FusionOS.Modules.Inventory.Application.Batches.Commands.ConsumeBatch;

/// <summary>Records that part of a batch has been used, reducing QuantityRemaining. Does NOT itself post an InventoryLedgerEntry — that stays the existing, already-working ledger path (see Batch.Consume's doc comment).</summary>
public sealed record ConsumeBatchCommand(Guid CompanyId, Guid BatchId, decimal Quantity)
    : ICommand<BatchDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.batch.update" };
    public string EntityType => nameof(Domain.Batches.Batch);
    public Guid EntityId => BatchId;
    public string Action => "Consumed";
}
