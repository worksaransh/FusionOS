using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.AccountingPeriods.Events;

/// <summary>
/// Raised when an AccountingPeriod moves Open -&gt; Closed — the point from which
/// CreateJournalEntryCommandHandler starts rejecting new journal entries whose
/// posting date falls inside this period (see AccountingPeriod's own class doc
/// comment for the "no period defined = unrestricted" opt-in behaviour).
/// </summary>
public sealed record AccountingPeriodClosed(Guid AccountingPeriodId, Guid CompanyId, DateTimeOffset PeriodStart, DateTimeOffset PeriodEnd) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
