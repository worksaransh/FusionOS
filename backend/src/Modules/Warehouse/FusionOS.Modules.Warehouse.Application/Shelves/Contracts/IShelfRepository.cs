namespace FusionOS.Modules.Warehouse.Application.Shelves.Contracts;

public interface IShelfRepository
{
    Task<Domain.Shelves.Shelf?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> RackExistsAsync(Guid companyId, Guid rackId, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(Guid companyId, Guid rackId, string code, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Shelves.Shelf shelf, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Shelves.Shelf>> ListAsync(Guid companyId, Guid rackId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid rackId, CancellationToken cancellationToken = default);
}
