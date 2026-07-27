namespace FusionOS.Modules.Quality.Application.Deviations.Contracts;

public interface IDeviationRequestRepository
{
    Task<Domain.Deviations.DeviationRequest?> GetByIdAsync(Guid companyId, Guid deviationRequestId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Deviations.DeviationRequest deviationRequest, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Deviations.DeviationRequest>> ListAsync(Guid companyId, Guid? inspectionId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid? inspectionId, CancellationToken cancellationToken = default);
}
