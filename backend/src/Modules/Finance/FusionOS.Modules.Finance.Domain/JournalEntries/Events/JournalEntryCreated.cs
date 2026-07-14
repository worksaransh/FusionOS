using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.JournalEntries.Events;

public sealed record JournalEntryCreated(Guid JournalEntryId, Guid CompanyId, decimal TotalAmount) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
