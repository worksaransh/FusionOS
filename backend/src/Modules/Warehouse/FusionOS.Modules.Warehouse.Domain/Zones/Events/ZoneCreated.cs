using FusionOS.SharedKernel;

namespace FusionOS.Modules.Warehouse.Domain.Zones.Events;

public sealed record ZoneCreated(Guid ZoneId, Guid CompanyId, Guid WarehouseId, string Code) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
