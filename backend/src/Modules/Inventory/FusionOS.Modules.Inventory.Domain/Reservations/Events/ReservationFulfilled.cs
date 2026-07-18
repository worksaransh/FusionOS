using FusionOS.SharedKernel;

namespace FusionOS.Modules.Inventory.Domain.Reservations.Events;

/// <summary>Raised once a reservation converts into an actual stock movement (e.g. a real Dispatch). No consumer this slice — the ledger entry itself is still posted by the existing Dispatch-consuming path, not by this event.</summary>
public sealed record ReservationFulfilled(Guid ReservationId, Guid CompanyId, Guid ProductId, Guid WarehouseId, decimal Quantity) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
