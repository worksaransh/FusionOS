namespace FusionOS.Modules.Sales.Application.SalesOrders.Contracts;

public interface ISalesOrderRepository
{
    Task<Domain.SalesOrders.SalesOrder?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.SalesOrders.SalesOrder order, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.SalesOrders.SalesOrder>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
}
