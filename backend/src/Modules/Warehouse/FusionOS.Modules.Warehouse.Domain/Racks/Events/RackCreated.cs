using FusionOS.SharedKernel;

namespace FusionOS.Modules.Warehouse.Domain.Racks.Events;

/// <summary>In-module notification only, no cross-module consumer yet — same as ZoneCreated/BinCreated.</summary>
public sealed record RackCreated(Guid RackId, Guid CompanyId, Guid ZoneId, string Code) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
