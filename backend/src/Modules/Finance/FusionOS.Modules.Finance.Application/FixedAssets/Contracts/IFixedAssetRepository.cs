namespace FusionOS.Modules.Finance.Application.FixedAssets.Contracts;

/// <summary>Mirrors ICostCenterRepository's shape (CodeExistsAsync/GetById/AddAsync/ListAsync/CountAsync), with ListAsync/CountAsync additionally filterable by IsDisposed/IsActive the way IBankStatementLineRepository's list is filterable by IsReconciled.</summary>
public interface IFixedAssetRepository
{
    Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default);

    Task<Domain.FixedAssets.FixedAsset?> GetByIdAsync(Guid companyId, Guid fixedAssetId, CancellationToken cancellationToken = default);

    Task AddAsync(Domain.FixedAssets.FixedAsset fixedAsset, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Domain.FixedAssets.FixedAsset>> ListAsync(Guid companyId, bool? isDisposed, bool? isActive, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountAsync(Guid companyId, bool? isDisposed, bool? isActive, CancellationToken cancellationToken = default);
}
