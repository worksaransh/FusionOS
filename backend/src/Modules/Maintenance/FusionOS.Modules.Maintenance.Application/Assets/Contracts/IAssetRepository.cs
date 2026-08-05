namespace FusionOS.Modules.Maintenance.Application.Assets.Contracts;

public interface IAssetRepository
{
    Task<Domain.Assets.Asset?> GetByIdAsync(Guid companyId, Guid assetId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid companyId, Guid assetId, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Assets.Asset asset, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Assets.Asset>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);

    /// <summary>
    /// Active assets whose WarrantyExpirationDate falls within the next <paramref name="withinDays"/>
    /// days from now (inclusive of already-expired ones, so a caller sees "needs attention" rather
    /// than losing track of a warranty that lapsed yesterday) — backs the expiring-warranty view
    /// called out in the warranty-tracking follow-up.
    /// </summary>
    Task<IReadOnlyList<Domain.Assets.Asset>> ListWithExpiringWarrantyAsync(Guid companyId, int withinDays, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk resolves the given Asset ids in one round trip — backs
    /// GetDowntimeMetricsReportQueryHandler, which needs the Code/Name for every
    /// distinct AssetId referenced by the period's completed maintenance requests
    /// rather than one GetByIdAsync call per distinct asset (a company with many
    /// assets under maintenance in the period was making one query per asset).
    /// </summary>
    Task<IReadOnlyList<Domain.Assets.Asset>> GetByIdsAsync(Guid companyId, IReadOnlyCollection<Guid> assetIds, CancellationToken cancellationToken = default);
}
