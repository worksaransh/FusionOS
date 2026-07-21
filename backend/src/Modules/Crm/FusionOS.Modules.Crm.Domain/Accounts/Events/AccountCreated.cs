using FusionOS.SharedKernel;

namespace FusionOS.Modules.Crm.Domain.Accounts.Events;

/// <summary>Raised when an account is captured. No consumer today — same "future hook" shape as LeadCreated.</summary>
public sealed record AccountCreated(Guid AccountId, Guid CompanyId, string Name) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
