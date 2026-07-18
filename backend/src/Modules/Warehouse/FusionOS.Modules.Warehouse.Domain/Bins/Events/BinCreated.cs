using FusionOS.SharedKernel;

namespace FusionOS.Modules.Warehouse.Domain.Bins.Events;

/// <summary>In-module notification only, no cross-module consumer yet — same as ZoneCreated/WarehouseCreated.</summary>
public sealed record BinCreated(Guid BinId, Guid CompanyId, Guid ZoneId, string Code) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
