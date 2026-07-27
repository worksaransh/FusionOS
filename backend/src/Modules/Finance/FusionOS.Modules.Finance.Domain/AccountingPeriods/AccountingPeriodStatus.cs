namespace FusionOS.Modules.Finance.Domain.AccountingPeriods;

/// <summary>Stored as text via EF value conversion, never a native PostgreSQL enum — 04_DATABASE_GUIDELINES.md §10 (same convention as JournalEntryStatus).</summary>
public enum AccountingPeriodStatus
{
    Open,
    Closed,
}
