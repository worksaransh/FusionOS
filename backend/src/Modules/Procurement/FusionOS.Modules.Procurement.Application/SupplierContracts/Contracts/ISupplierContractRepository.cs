namespace FusionOS.Modules.Procurement.Application.SupplierContracts.Contracts;

public interface ISupplierContractRepository
{
    Task<Domain.SupplierContracts.SupplierContract?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.SupplierContracts.SupplierContract contract, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.SupplierContracts.SupplierContract>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);
}
