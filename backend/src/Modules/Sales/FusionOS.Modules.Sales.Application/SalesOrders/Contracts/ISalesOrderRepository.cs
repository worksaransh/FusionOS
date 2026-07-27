namespace FusionOS.Modules.Sales.Application.SalesOrders.Contracts;

public interface ISalesOrderRepository
{
    Task<Domain.SalesOrders.SalesOrder?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.SalesOrders.SalesOrder order, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.SalesOrders.SalesOrder>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>Customer Portal support (2026-07-22) - same CustomerId-scoped shape as IQuotationRepository.ListByCustomerAsync; see its doc comment.</summary>
    Task<IReadOnlyList<Domain.SalesOrders.SalesOrder>> ListByCustomerAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Customer Portal support (2026-07-22) - see ListByCustomerAsync.</summary>
    Task<int> CountByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
}
