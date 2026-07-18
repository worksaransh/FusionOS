namespace FusionOS.Modules.Sales.Domain.Quotations;

/// <summary>Stored as text via EF value conversion, never a native PostgreSQL enum — 04_DATABASE_GUIDELINES.md §10.</summary>
public enum QuotationStatus
{
    Draft,
    Accepted,
    Rejected,
    Converted,
}
