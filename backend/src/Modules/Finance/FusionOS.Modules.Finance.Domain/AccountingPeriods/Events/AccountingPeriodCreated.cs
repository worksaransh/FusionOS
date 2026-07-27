using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.AccountingPeriods.Events;

public sealed record AccountingPeriodCreated(Guid AccountingPeriodId, Guid CompanyId, DateTimeOffset PeriodStart, DateTimeOffset PeriodEnd) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
