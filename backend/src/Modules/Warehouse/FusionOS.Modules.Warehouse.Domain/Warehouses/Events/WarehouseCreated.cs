using FusionOS.SharedKernel;

namespace FusionOS.Modules.Warehouse.Domain.Warehouses.Events;

public sealed record WarehouseCreated(Guid WarehouseId, Guid CompanyId, string Code) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
