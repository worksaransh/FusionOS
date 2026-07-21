using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Batches.Contracts;

namespace FusionOS.Modules.Inventory.Application.Batches.Commands.CreateBatch;

public sealed record CreateBatchCommand(Guid CompanyId, Guid ProductId, string BatchNumber, decimal QuantityReceived, DateTimeOffset? ExpiryDate)
    : ICommand<BatchDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.batch.create" };
    public string EntityType => nameof(Domain.Batches.Batch);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
