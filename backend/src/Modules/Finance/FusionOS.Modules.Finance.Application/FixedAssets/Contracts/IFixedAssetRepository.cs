namespace FusionOS.Modules.Finance.Application.FixedAssets.Contracts;

/// <summary>Mirrors ICostCenterRepository's shape (CodeExistsAsync/GetById/AddAsync/ListAsync/CountAsync), with ListAsync/CountAsync additionally filterable by IsDisposed/IsActive the way IBankStatementLineRepository's list is filterable by IsReconciled.</summary>
public interface IFixedAssetRepository
{
    Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default);

    Task<Domain.FixedAssets.FixedAsset?> GetByIdAsync(Guid companyId, Guid fixedAssetId, CancellationToken cancellationToken = default);

    Task AddAsync(Domain.FixedAssets.FixedAsset fixedAsset, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Domain.FixedAssets.FixedAsset>> ListAsync(Guid companyId, bool? isDisposed, bool? isActive, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountAsync(Guid companyId, bool? isDisposed, bool? isActive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Every fixed asset for this company, unpaged — backs
    /// GetCashFlowStatementReportQuery (2026-07-22), which needs to scan every
    /// asset's AcquisitionDate/DisposedDate to find period activity, not a
    /// page of it. Same "full unpaged list for a canned report to filter
    /// itself" shape as IAccountRepository.ListAllAsync (used by the Balance
    /// Sheet/P&amp;L reports).
    /// </summary>
    Task<IReadOnlyList<Domain.FixedAssets.FixedAsset>> ListAllAsync(Guid companyId, CancellationToken cancellationToken = default);
}
