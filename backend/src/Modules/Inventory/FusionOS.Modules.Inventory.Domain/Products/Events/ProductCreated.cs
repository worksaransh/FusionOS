using FusionOS.SharedKernel;

namespace FusionOS.Modules.Inventory.Domain.Products.Events;

public sealed record ProductCreated(Guid ProductId, Guid CompanyId, string Sku) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
