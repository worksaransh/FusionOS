namespace FusionOS.Modules.Maintenance.Application.Assets.Contracts;

public interface IAssetRepository
{
    Task<Domain.Assets.Asset?> GetByIdAsync(Guid companyId, Guid assetId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid companyId, Guid assetId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Assets.Asset asset, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Assets.Asset>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
