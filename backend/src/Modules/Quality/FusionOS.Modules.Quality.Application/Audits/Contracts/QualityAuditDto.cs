namespace FusionOS.Modules.Quality.Application.Audits.Contracts;

public sealed record QualityAuditDto(
    Guid Id,
    string AuditType,
    DateTimeOffset ScheduledDate,
    string Auditor,
    string Scope,
    string Status,
    IReadOnlyList<AuditFindingDto> Findings);

/// <summary>Single place that turns a QualityAudit aggregate into its DTO, shared by every handler that returns one.</summary>
public static class QualityAuditMapper
{
    public static QualityAuditDto ToDto(Domain.Audits.QualityAudit audit) => new(
        audit.Id,
        audit.AuditType.ToString(),
        audit.ScheduledDate,
        audit.Auditor,
        audit.Scope,
        audit.Status.ToString(),
        audit.Findings.Select(f => new AuditFindingDto(f.Id, f.Description, f.Severity.ToString(), f.IsResolved)).ToList());
}
