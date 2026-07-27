namespace FusionOS.Modules.Quality.Application.Certificates.Contracts;

/// <summary>
/// Lightweight listing shape for ListCertificatesQuery — deliberately without the embedded
/// Inspection data QualityCertificateDto carries (that would mean an extra Inspection lookup
/// per row for a list view that just needs "what's been issued, to whom, when"). Fetch the
/// full <see cref="QualityCertificateDto"/> via GetCertificateByInspectionIdQuery once a
/// specific certificate needs rendering.
/// </summary>
public sealed record QualityCertificateSummaryDto(Guid Id, Guid InspectionId, string CertificateNumber, DateTimeOffset IssuedAt, Guid IssuedByUserId);

public static class QualityCertificateSummaryMapper
{
    public static QualityCertificateSummaryDto ToSummaryDto(Domain.Certificates.QualityCertificate certificate) => new(
        certificate.Id, certificate.InspectionId, certificate.CertificateNumber, certificate.IssuedAt, certificate.IssuedByUserId);
}
