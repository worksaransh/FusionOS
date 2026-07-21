using FusionOS.SharedKernel;

namespace FusionOS.Modules.Crm.Domain.Contacts.Events;

/// <summary>Raised when a contact is captured. No consumer today — same "future hook" shape as LeadCreated.</summary>
public sealed record ContactCreated(Guid ContactId, Guid CompanyId, string Name) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
