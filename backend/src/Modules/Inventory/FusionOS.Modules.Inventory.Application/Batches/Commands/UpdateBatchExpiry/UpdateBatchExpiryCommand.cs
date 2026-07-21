using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Batches.Contracts;

namespace FusionOS.Modules.Inventory.Application.Batches.Commands.UpdateBatchExpiry;

/// <summary>Corrects a batch's recorded expiry — e.g. a data-entry fix, or a supplier-confirmed shelf-life extension. NewExpiry may be null to clear it.</summary>
public sealed record UpdateBatchExpiryCommand(Guid CompanyId, Guid BatchId, DateTimeOffset? NewExpiry)
    : ICommand<BatchDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.batch.update" };
    public string EntityType => nameof(Domain.Batches.Batch);
    public Guid EntityId => BatchId;
    public string Action => "ExpiryAdjusted";
}
