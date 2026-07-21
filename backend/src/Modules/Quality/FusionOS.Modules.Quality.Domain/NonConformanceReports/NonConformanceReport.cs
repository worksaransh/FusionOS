using FusionOS.SharedKernel;
using FusionOS.Modules.Quality.Domain.NonConformanceReports.Events;

namespace FusionOS.Modules.Quality.Domain.NonConformanceReports;

/// <summary>
/// Phase 5 — Quality: a Non-Conformance Report (NCR) records a defect or deviation found —
/// either during a formal <see cref="Inspections.Inspection"/> (<see cref="InspectionId"/>
/// set) or observed standalone (InspectionId null; e.g. a defect noticed on the shop floor
/// outside any inspection). InspectionId is a same-module reference (Inspection lives in
/// this same Quality module), existence-validated by the command handler when supplied —
/// same convention as MaintenanceRequest validating AssetId via IAssetRepository
/// (Maintenance module). Left null, it never touches Inspection at all.
///
/// Lifecycle is a simple forward-only progression, Open -> UnderReview -> Closed (or Open ->
/// Closed directly, skipping formal review for a trivial NCR) — never backward, and never
/// mutable once Closed, same discipline as MaintenanceRequest's Open -> InProgress ->
/// Completed and Inspection's resolve-once RecordResults.
/// </summary>
public sealed class NonConformanceReport : TenantAggregateRoot
{
    public Guid? InspectionId { get; private set; }
    public string Description { get; private set; } = default!;
    public NonConformanceReportSeverity Severity { get; private set; }
    public NonConformanceReportStatus Status { get; private set; }
    public Guid RaisedByUserId { get; private set; }
    public DateTimeOffset RaisedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }

    private NonConformanceReport() { }

    public static NonConformanceReport Create(Guid companyId, Guid? inspectionId, string description, NonConformanceReportSeverity severity, Guid raisedByUserId)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A description of the non-conformance is required.", nameof(description));
        if (raisedByUserId == Guid.Empty)
            throw new ArgumentException("Raised-by user id is required.", nameof(raisedByUserId));
        if (inspectionId == Guid.Empty)
            throw new ArgumentException("Inspection id, when supplied, cannot be empty — omit it entirely for a standalone NCR.", nameof(inspectionId));

        var ncr = new NonConformanceReport
        {
            CompanyId = companyId,
            InspectionId = inspectionId,
            Description = description.Trim(),
            Severity = severity,
            Status = NonConformanceReportStatus.Open,
            RaisedByUserId = raisedByUserId,
            RaisedAt = DateTimeOffset.UtcNow,
        };

        ncr.Raise(new NonConformanceReportCreated(ncr.Id, companyId, inspectionId, severity.ToString()));
        return ncr;
    }

    /// <summary>
    /// Moves the NCR forward — Open -> UnderReview -> Closed, or Open -> Closed directly.
    /// Never backward (including re-selecting the current status) and never once already
    /// Closed. Raises <see cref="NonConformanceReportClosed"/> only on the transition into
    /// Closed.
    /// </summary>
    public void UpdateStatus(NonConformanceReportStatus newStatus)
    {
        if (Status == NonConformanceReportStatus.Closed)
            throw new InvalidOperationException("A closed non-conformance report's status cannot be changed.");
        if (newStatus == Status)
            throw new InvalidOperationException($"The non-conformance report is already {Status}.");
        if (newStatus == NonConformanceReportStatus.Open)
            throw new InvalidOperationException("A non-conformance report cannot move back to Open once raised.");

        Status = newStatus;
        if (newStatus == NonConformanceReportStatus.Closed)
        {
            ClosedAt = DateTimeOffset.UtcNow;
            Raise(new NonConformanceReportClosed(Id, CompanyId, InspectionId, Severity.ToString()));
        }
    }
}
