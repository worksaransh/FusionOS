namespace FusionOS.Modules.Finance.Domain.Accounts;

/// <summary>Stored as text via EF value conversion, never a native PostgreSQL enum — 04_DATABASE_GUIDELINES.md §10.</summary>
public enum AccountType
{
    Asset,
    Liability,
    Equity,
    Revenue,
    Expense,
}
