using FusionOS.SharedKernel;

namespace FusionOS.Modules.Warehouse.Domain.CycleCounts.Events;

/// <summary>In-module notification only, no cross-module consumer yet — same as ZoneCreated/BinCreated.</summary>
public sealed record CycleCountStarted(Guid CycleCountId, Guid CompanyId, Guid WarehouseId, Guid ZoneId, Guid BinId, Guid ProductId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
