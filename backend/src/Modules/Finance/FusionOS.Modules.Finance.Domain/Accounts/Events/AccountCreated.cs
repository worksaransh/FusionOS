using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.Accounts.Events;

public sealed record AccountCreated(Guid AccountId, Guid CompanyId, string Code, AccountType AccountType) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
