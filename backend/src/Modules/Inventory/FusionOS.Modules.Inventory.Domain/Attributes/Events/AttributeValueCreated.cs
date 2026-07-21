using FusionOS.SharedKernel;

namespace FusionOS.Modules.Inventory.Domain.Attributes.Events;

public sealed record AttributeValueCreated(Guid AttributeValueId, Guid CompanyId, Guid AttributeDefinitionId, string Value) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
