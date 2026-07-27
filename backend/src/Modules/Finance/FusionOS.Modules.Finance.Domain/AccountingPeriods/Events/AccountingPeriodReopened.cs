using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.AccountingPeriods.Events;

/// <summary>Raised when a previously Closed AccountingPeriod moves back to Open — new journal entries posting into it are no longer blocked from that point on.</summary>
public sealed record AccountingPeriodReopened(Guid AccountingPeriodId, Guid CompanyId, DateTimeOffset PeriodStart, DateTimeOffset PeriodEnd) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
