namespace FusionOS.Modules.Quality.Application.Inspections.Contracts;

public interface IInspectionRepository
{
    Task<Domain.Inspections.Inspection?> GetByIdAsync(Guid companyId, Guid inspectionId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Inspections.Inspection inspection, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Inspections.Inspection>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
}
