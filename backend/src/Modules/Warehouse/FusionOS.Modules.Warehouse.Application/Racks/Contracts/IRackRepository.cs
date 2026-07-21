namespace FusionOS.Modules.Warehouse.Application.Racks.Contracts;

public interface IRackRepository
{
    Task<Domain.Racks.Rack?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ZoneExistsAsync(Guid companyId, Guid zoneId, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(Guid companyId, Guid zoneId, string code, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Racks.Rack rack, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Racks.Rack>> ListAsync(Guid companyId, Guid zoneId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid zoneId, CancellationToken cancellationToken = default);
}
