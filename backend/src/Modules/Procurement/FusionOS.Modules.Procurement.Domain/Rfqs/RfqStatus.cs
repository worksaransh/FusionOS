namespace FusionOS.Modules.Procurement.Domain.Rfqs;

/// <summary>Stored as text via EF value conversion, never a native PostgreSQL enum — 04_DATABASE_GUIDELINES.md §10.</summary>
public enum RfqStatus
{
    Draft,
    Sent,
    Awarded,
}
