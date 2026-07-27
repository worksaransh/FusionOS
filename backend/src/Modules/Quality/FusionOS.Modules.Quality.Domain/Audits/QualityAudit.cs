using FusionOS.SharedKernel;

namespace FusionOS.Modules.Quality.Domain.Audits;

/// <summary>
/// A scheduled quality audit (internal or external/third-party) with a free-text scope
/// description, moving forward-only Scheduled -> InProgress -> Completed (same discipline as
/// Inspections/CorrectiveActions/MaintenanceRequest in this module family) and accumulating
/// zero or more <see cref="AuditFinding"/> child entities as the audit proceeds.
///
/// Auditor is a plain string (a name), not a Core User reference: an external auditor is not
/// a FusionOS user at all, and even an internal audit may be led by someone outside the system
/// (e.g. a contracted quality consultant) — same reasoning MaintenanceRequest.AssignedToUserId
/// does NOT apply here since there is no guaranteed opaque Core User id to hold in every case.
/// </summary>
public sealed class QualityAudit : TenantAggregateRoot
{
    private readonly List<AuditFinding> _findings = new();

    public QualityAuditType AuditType { get; private set; }
    public DateTimeOffset ScheduledDate { get; private set; }
    public string Auditor { get; private set; } = default!;
    public string Scope { get; private set; } = default!;
    public QualityAuditStatus Status { get; private set; }
    public IReadOnlyList<AuditFinding> Findings => _findings.AsReadOnly();

    private QualityAudit() { }

    public static QualityAudit Create(Guid companyId, QualityAuditType auditType, DateTimeOffset scheduledDate, string auditor, string scope)
    {
        if (string.IsNullOrWhiteSpace(auditor))
            throw new ArgumentException("Auditor is required.", nameof(auditor));
        if (string.IsNullOrWhiteSpace(scope))
            throw new ArgumentException("Audit scope is required.", nameof(scope));
        if (scheduledDate == default)
            throw new ArgumentException("Scheduled date is required.", nameof(scheduledDate));

        return new QualityAudit
        {
            CompanyId = companyId,
            AuditType = auditType,
            ScheduledDate = scheduledDate,
            Auditor = auditor.Trim(),
            Scope = scope.Trim(),
            Status = QualityAuditStatus.Scheduled,
        };
    }

    /// <summary>Marks the audit as underway. Requires it to still be Scheduled.</summary>
    public void Start()
    {
        if (Status != QualityAuditStatus.Scheduled)
            throw new InvalidOperationException($"Only a Scheduled quality audit can be started (current status: {Status}).");

        Status = QualityAuditStatus.InProgress;
    }

    /// <summary>Marks the audit as finished. Requires it to be InProgress.</summary>
    public void Complete()
    {
        if (Status != QualityAuditStatus.InProgress)
            throw new InvalidOperationException($"Only an InProgress quality audit can be completed (current status: {Status}).");

        Status = QualityAuditStatus.Completed;
    }

    /// <summary>
    /// Records a finding against this audit. Allowed at any status prior to Completed — a
    /// finding can be noted the moment it's observed, whether or not the audit has formally
    /// been marked InProgress yet, same "don't force a status transition just to record data"
    /// latitude Inspection gives none of but MaintenanceRequest's notes-style fields do.
    /// </summary>
    public void AddFinding(string description, AuditFindingSeverity severity)
    {
        if (Status == QualityAuditStatus.Completed)
            throw new InvalidOperationException("Findings cannot be added to a Completed quality audit.");

        _findings.Add(AuditFinding.Create(description, severity));
    }

    /// <summary>Marks a specific finding as resolved. Throws if no such finding exists on this audit.</summary>
    public void ResolveFinding(Guid findingId)
    {
        var finding = _findings.FirstOrDefault(f => f.Id == findingId)
            ?? throw new InvalidOperationException($"Finding '{findingId}' was not found on this quality audit.");

        finding.Resolve();
    }
}
