namespace FusionOS.Modules.Sales.Application.Commissions.Contracts;

public interface ISalesCommissionRateRepository
{
    Task<Domain.Commissions.SalesCommissionRate?> GetByUserIdAsync(Guid companyId, Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Commissions.SalesCommissionRate rate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Commissions.SalesCommissionRate>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
}
