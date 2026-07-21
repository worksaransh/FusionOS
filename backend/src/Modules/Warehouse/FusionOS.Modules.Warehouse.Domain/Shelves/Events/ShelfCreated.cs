using FusionOS.SharedKernel;

namespace FusionOS.Modules.Warehouse.Domain.Shelves.Events;

/// <summary>In-module notification only, no cross-module consumer yet — same as RackCreated/ZoneCreated/BinCreated.</summary>
public sealed record ShelfCreated(Guid ShelfId, Guid CompanyId, Guid RackId, string Code) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
