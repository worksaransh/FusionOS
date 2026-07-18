namespace FusionOS.Modules.Sales.Application.PriceLists.Contracts;

public interface IPriceListRepository
{
    Task<Domain.PriceLists.PriceList?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid companyId, Guid priceListId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.PriceLists.PriceList priceList, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.PriceLists.PriceList>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
}
