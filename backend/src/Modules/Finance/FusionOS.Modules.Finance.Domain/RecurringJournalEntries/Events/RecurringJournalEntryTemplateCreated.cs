using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.RecurringJournalEntries.Events;

public sealed record RecurringJournalEntryTemplateCreated(Guid RecurringJournalEntryTemplateId, Guid CompanyId, string Reference, string Frequency) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
