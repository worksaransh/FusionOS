namespace FusionOS.Modules.Quality.Application.NonConformanceReports.Contracts;

public interface INonConformanceReportRepository
{
    Task<Domain.NonConformanceReports.NonConformanceReport?> GetByIdAsync(Guid companyId, Guid nonConformanceReportId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.NonConformanceReports.NonConformanceReport report, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.NonConformanceReports.NonConformanceReport>> ListAsync(Guid companyId, Guid? inspectionId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid? inspectionId, CancellationToken cancellationToken = default);
}
