namespace FusionOS.Modules.Sales.Application.Dispatches.Contracts;

public interface IDispatchRepository
{
    Task AddAsync(Domain.Dispatches.Dispatch dispatch, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Dispatches.Dispatch>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
}
