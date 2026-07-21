namespace FusionOS.Modules.Inventory.Application.Batches.Contracts;

public interface IBatchRepository
{
    Task<Domain.Batches.Batch?> GetByIdAsync(Guid companyId, Guid batchId, CancellationToken cancellationToken = default);
    Task<bool> BatchNumberExistsAsync(Guid companyId, Guid productId, string batchNumber, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Batches.Batch batch, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Batches.Batch>> ListByProductAsync(Guid companyId, Guid productId, DateTimeOffset? expiringBefore, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountByProductAsync(Guid companyId, Guid productId, DateTimeOffset? expiringBefore, CancellationToken cancellationToken = default);
}
