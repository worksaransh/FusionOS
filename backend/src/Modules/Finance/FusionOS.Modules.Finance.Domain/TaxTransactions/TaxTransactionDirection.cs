namespace FusionOS.Modules.Finance.Domain.TaxTransactions;

/// <summary>Stored as text via EF value conversion, never a native PostgreSQL enum — 04_DATABASE_GUIDELINES.md §10.</summary>
public enum TaxTransactionDirection
{
    /// <summary>Tax collected on a sale (owed to the government).</summary>
    Output,

    /// <summary>Tax paid on a purchase (potentially claimable as input tax credit).</summary>
    Input,
}
