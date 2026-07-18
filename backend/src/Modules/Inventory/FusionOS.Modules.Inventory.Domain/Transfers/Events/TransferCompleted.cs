using FusionOS.SharedKernel;

namespace FusionOS.Modules.Inventory.Domain.Transfers.Events;

public sealed record TransferCompleted(Guid TransferId, Guid CompanyId, Guid ProductId, Guid SourceWarehouseId, Guid DestinationWarehouseId, decimal Quantity) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
