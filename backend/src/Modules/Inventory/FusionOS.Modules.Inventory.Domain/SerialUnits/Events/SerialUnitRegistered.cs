using FusionOS.SharedKernel;

namespace FusionOS.Modules.Inventory.Domain.SerialUnits.Events;

public sealed record SerialUnitRegistered(Guid SerialUnitId, Guid CompanyId, Guid ProductId, string SerialNumber) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
