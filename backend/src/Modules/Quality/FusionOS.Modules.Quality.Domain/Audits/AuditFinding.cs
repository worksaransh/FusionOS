namespace FusionOS.Modules.Quality.Domain.Audits;

/// <summary>
/// One finding recorded against a <see cref="QualityAudit"/>. Same documented no-audit/tenant-columns
/// exception as every other line entity owned by a parent aggregate (e.g. Inspections.InspectionItem)
/// — its lifecycle is owned entirely by the parent QualityAudit, never queried or referenced on its own.
/// </summary>
public sealed class AuditFinding
{
    public Guid Id { get; private set; }
    public string Description { get; private set; } = default!;
    public AuditFindingSeverity Severity { get; private set; }
    public bool IsResolved { get; private set; }

    private AuditFinding() { }

    internal static AuditFinding Create(string description, AuditFindingSeverity severity)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Finding description is required.", nameof(description));

        return new AuditFinding
        {
            Id = Guid.NewGuid(),
            Description = description.Trim(),
            Severity = severity,
            IsResolved = false,
        };
    }

    internal void Resolve()
    {
        IsResolved = true;
    }
}
