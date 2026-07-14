namespace FusionOS.Modules.Sales.Application.Customers.Contracts;

public interface ICustomerRepository
{
    Task<Domain.Customers.Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Customers.Customer customer, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Customers.Customer>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
