using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.BankAccounts.Events;

public sealed record BankAccountCreated(Guid BankAccountId, Guid CompanyId, string Code) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
