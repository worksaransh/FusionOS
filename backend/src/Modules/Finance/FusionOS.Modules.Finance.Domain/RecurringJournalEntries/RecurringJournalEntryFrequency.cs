namespace FusionOS.Modules.Finance.Domain.RecurringJournalEntries;

/// <summary>Stored as text via EF value conversion, never a native PostgreSQL enum — 04_DATABASE_GUIDELINES.md §10 (same convention as JournalEntryStatus/AccountingPeriodStatus).</summary>
public enum RecurringJournalEntryFrequency
{
    Monthly,
    Quarterly,
    Annual,
}
