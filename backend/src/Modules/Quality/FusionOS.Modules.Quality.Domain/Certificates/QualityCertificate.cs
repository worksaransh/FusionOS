using FusionOS.SharedKernel;

namespace FusionOS.Modules.Quality.Domain.Certificates;

/// <summary>
/// A Certificate of Analysis/Conformance (COA/COC) issued for a Passed
/// <see cref="Inspections.Inspection"/>. <see cref="InspectionId"/> is a same-module
/// reference (Inspection lives in this same Quality module), existence- and
/// status-validated by GenerateCertificateOfAnalysisCommandHandler before this is ever
/// created — same convention as NonConformanceReport.InspectionId.
///
/// <b>Scope decision (documented per the task's own framing):</b> a certificate is
/// fundamentally a derived view over an existing Inspection's characteristic/result data —
/// no new inspection state is captured here. It is still persisted, rather than computed
/// on demand, because "was a certificate ever issued for this inspection, and when, under
/// what number" is exactly the kind of fact a real audit trail needs to answer — a purely
/// computed view has no way to answer "has this already been issued" for idempotency (see
/// the handler, which rejects a second certificate for the same InspectionId). The
/// characteristic/result data itself is NOT duplicated onto this aggregate: callers get it
/// by combining this record with the Inspection it points to (see QualityCertificateDto),
/// so there is nothing here to go stale if the Inspection could ever be amended (it can't —
/// RecordResults only runs once, per Inspection.RecordResults' own Pending-only guard).
///
/// Generating an actual PDF is out of scope for this slice — QualityCertificateDto returns
/// structured JSON (certificate number/dates plus the full inspection characteristic/result
/// data) that a caller/frontend can render and print; see CertificatesPanel.tsx.
/// </summary>
public sealed class QualityCertificate : TenantAggregateRoot
{
    public Guid InspectionId { get; private set; }
    public string CertificateNumber { get; private set; } = default!;
    public DateTimeOffset IssuedAt { get; private set; }
    public Guid IssuedByUserId { get; private set; }

    private QualityCertificate() { }

    public static QualityCertificate Create(Guid companyId, Guid inspectionId, Guid issuedByUserId)
    {
        if (inspectionId == Guid.Empty)
            throw new ArgumentException("Inspection id is required.", nameof(inspectionId));
        if (issuedByUserId == Guid.Empty)
            throw new ArgumentException("Issued-by user id is required.", nameof(issuedByUserId));

        var issuedAt = DateTimeOffset.UtcNow;
        return new QualityCertificate
        {
            CompanyId = companyId,
            InspectionId = inspectionId,
            CertificateNumber = GenerateCertificateNumber(issuedAt),
            IssuedAt = issuedAt,
            IssuedByUserId = issuedByUserId,
        };
    }

    /// <summary>
    /// "COA-{yyyyMMdd}-{8 hex chars}" — sequential-looking and date-scoped for a human
    /// reading a printed certificate, but actually GUID-derived (not a real per-company
    /// sequence counter) so no coordination/locking is needed to avoid a collision across
    /// concurrent generations. A real sequential register per company is a reasonable
    /// follow-up if a numbering scheme becomes a compliance requirement; this is the honest,
    /// simple scope for this slice.
    /// </summary>
    private static string GenerateCertificateNumber(DateTimeOffset issuedAt) =>
        $"COA-{issuedAt:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}
