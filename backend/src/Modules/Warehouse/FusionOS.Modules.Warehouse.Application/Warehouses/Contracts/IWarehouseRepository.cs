using FusionOS.Modules.Warehouse.Domain.Warehouses;

namespace FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;

public interface IWarehouseRepository
{
    Task<Domain.Warehouses.Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Warehouses.Warehouse warehouse, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Warehouses.Warehouse>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
