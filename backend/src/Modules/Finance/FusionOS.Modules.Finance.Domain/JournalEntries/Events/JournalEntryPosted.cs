using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.JournalEntries.Events;

/// <summary>
/// Raised when a JournalEntry moves Draft -> Posted, i.e. the point at which it
/// actually affects the General Ledger. A candidate cross-module integration
/// event once BI/Reporting consumers exist (03_SYSTEM_ARCHITECTURE.md §4.2) —
/// no consumer exists yet, same documented gap as GoodsReceiptLineReceived.
/// </summary>
public sealed record JournalEntryPosted(Guid JournalEntryId, Guid CompanyId, decimal TotalAmount) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
