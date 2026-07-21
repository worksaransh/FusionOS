using FusionOS.SharedKernel;

namespace FusionOS.Modules.Inventory.Domain.Attributes.Events;

public sealed record AttributeDefinitionCreated(Guid AttributeDefinitionId, Guid CompanyId, string Name) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
