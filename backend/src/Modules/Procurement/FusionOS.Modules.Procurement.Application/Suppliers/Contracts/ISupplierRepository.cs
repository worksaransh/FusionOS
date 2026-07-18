namespace FusionOS.Modules.Procurement.Application.Suppliers.Contracts;

public interface ISupplierRepository
{
    Task<Domain.Suppliers.Supplier?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid companyId, Guid supplierId, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Suppliers.Supplier supplier, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Suppliers.Supplier>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
