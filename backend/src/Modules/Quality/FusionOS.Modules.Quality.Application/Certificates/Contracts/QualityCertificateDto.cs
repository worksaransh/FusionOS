using FusionOS.Modules.Quality.Application.Inspections.Contracts;

namespace FusionOS.Modules.Quality.Application.Certificates.Contracts;

/// <summary>
/// A generated Certificate of Analysis/Conformance, bundled with the full inspection
/// characteristic/result data (<see cref="Inspection"/>) needed to render/print a COA
/// document — see QualityCertificate's doc comment for why that data isn't duplicated onto
/// the certificate itself. No PDF is generated here; this structured shape is the deliverable
/// for this slice (see CertificatesPanel.tsx on the frontend for the printable rendering).
/// </summary>
public sealed record QualityCertificateDto(
    Guid Id,
    Guid InspectionId,
    string CertificateNumber,
    DateTimeOffset IssuedAt,
    Guid IssuedByUserId,
    InspectionDto Inspection);

/// <summary>Single place that turns a QualityCertificate aggregate (plus its Inspection) into its DTO.</summary>
public static class QualityCertificateMapper
{
    public static QualityCertificateDto ToDto(Domain.Certificates.QualityCertificate certificate, InspectionDto inspection) => new(
        certificate.Id,
        certificate.InspectionId,
        certificate.CertificateNumber,
        certificate.IssuedAt,
        certificate.IssuedByUserId,
        inspection);
}
