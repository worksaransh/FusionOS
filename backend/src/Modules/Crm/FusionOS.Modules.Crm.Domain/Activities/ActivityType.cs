namespace FusionOS.Modules.Crm.Domain.Activities;

/// <summary>Stored as text via EF value conversion, never a native PostgreSQL enum — 04_DATABASE_GUIDELINES.md §10.</summary>
public enum ActivityType
{
    Call,
    Email,
    Meeting,
    Note,
}
