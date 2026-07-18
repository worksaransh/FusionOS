namespace FusionOS.Modules.Warehouse.Application.CycleCounts.Contracts;

public interface ICycleCountRepository
{
    Task<Domain.CycleCounts.CycleCount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> BinExistsAsync(Guid companyId, Guid zoneId, Guid binId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.CycleCounts.CycleCount cycleCount, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.CycleCounts.CycleCount>> ListAsync(Guid companyId, Guid warehouseId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid warehouseId, CancellationToken cancellationToken = default);
}
