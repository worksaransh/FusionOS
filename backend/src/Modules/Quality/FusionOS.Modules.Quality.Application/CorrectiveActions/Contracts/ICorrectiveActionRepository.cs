namespace FusionOS.Modules.Quality.Application.CorrectiveActions.Contracts;

public interface ICorrectiveActionRepository
{
    Task<Domain.CorrectiveActions.CorrectiveAction?> GetByIdAsync(Guid companyId, Guid correctiveActionId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.CorrectiveActions.CorrectiveAction correctiveAction, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.CorrectiveActions.CorrectiveAction>> ListAsync(Guid companyId, Guid? nonConformanceReportId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid? nonConformanceReportId, CancellationToken cancellationToken = default);
}
