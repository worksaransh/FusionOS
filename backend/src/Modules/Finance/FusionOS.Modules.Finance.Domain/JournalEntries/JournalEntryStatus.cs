namespace FusionOS.Modules.Finance.Domain.JournalEntries;

/// <summary>Stored as text via EF value conversion, never a native PostgreSQL enum — 04_DATABASE_GUIDELINES.md §10.</summary>
public enum JournalEntryStatus
{
    Draft,
    Posted,
}
