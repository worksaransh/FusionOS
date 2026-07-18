using FusionOS.SharedKernel;

namespace FusionOS.Modules.Warehouse.Domain.PickLists.Events;

/// <summary>In-module notification only, no cross-module consumer yet — same as ZoneCreated/BinCreated.</summary>
public sealed record PickListCreated(Guid PickListId, Guid CompanyId, Guid WarehouseId, Guid SalesOrderId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
