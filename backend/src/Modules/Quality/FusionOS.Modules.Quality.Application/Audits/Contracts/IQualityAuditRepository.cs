namespace FusionOS.Modules.Quality.Application.Audits.Contracts;

public interface IQualityAuditRepository
{
    Task<Domain.Audits.QualityAudit?> GetByIdAsync(Guid companyId, Guid qualityAuditId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Audits.QualityAudit qualityAudit, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Audits.QualityAudit>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
}
