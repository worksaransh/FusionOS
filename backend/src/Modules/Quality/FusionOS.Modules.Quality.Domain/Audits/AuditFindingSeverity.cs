namespace FusionOS.Modules.Quality.Domain.Audits;

/// <summary>
/// Deliberately its own enum rather than reusing NonConformanceReports.NonConformanceReportSeverity:
/// an audit finding is a different concept from an NCR (a finding may never become an NCR at all,
/// e.g. a minor documentation gap noted during an internal audit), and coupling this module's
/// Audits slice to the NonConformanceReports slice's enum for no functional benefit would be a
/// needless internal dependency. The values are intentionally the same shape (Minor/Major/Critical)
/// since that is the common industry vocabulary for both. Stored as text via EF value conversion,
/// never a native PostgreSQL enum — 04_DATABASE_GUIDELINES.md §10.
/// </summary>
public enum AuditFindingSeverity
{
    Minor,
    Major,
    Critical,
}
