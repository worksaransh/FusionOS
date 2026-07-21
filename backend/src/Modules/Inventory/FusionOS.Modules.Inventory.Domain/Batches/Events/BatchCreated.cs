using FusionOS.SharedKernel;

namespace FusionOS.Modules.Inventory.Domain.Batches.Events;

public sealed record BatchCreated(Guid BatchId, Guid CompanyId, Guid ProductId, string BatchNumber, decimal QuantityReceived) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
