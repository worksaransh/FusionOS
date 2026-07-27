namespace FusionOS.Modules.Quality.Application.Certificates.Contracts;

public interface IQualityCertificateRepository
{
    Task<Domain.Certificates.QualityCertificate?> GetByIdAsync(Guid companyId, Guid certificateId, CancellationToken cancellationToken = default);

    /// <summary>Used both to render a certificate by its inspection, and by GenerateCertificateOfAnalysisCommandHandler to enforce one certificate per inspection (idempotency).</summary>
    Task<Domain.Certificates.QualityCertificate?> GetByInspectionIdAsync(Guid companyId, Guid inspectionId, CancellationToken cancellationToken = default);

    Task AddAsync(Domain.Certificates.QualityCertificate certificate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Certificates.QualityCertificate>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
}
