using FusionOS.SharedKernel;

namespace FusionOS.Modules.Inventory.Domain.Reservations.Events;

public sealed record ReservationCreated(Guid ReservationId, Guid CompanyId, Guid ProductId, Guid WarehouseId, decimal Quantity) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
