namespace FusionOS.Modules.Warehouse.Application.Zones.Contracts;

public interface IZoneRepository
{
    Task<Domain.Zones.Zone?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> WarehouseExistsAsync(Guid companyId, Guid warehouseId, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(Guid companyId, Guid warehouseId, string code, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Zones.Zone zone, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Zones.Zone>> ListAsync(Guid companyId, Guid warehouseId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid warehouseId, CancellationToken cancellationToken = default);
}
