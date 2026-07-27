namespace FusionOS.Modules.Procurement.Application.SupplierRatings.Contracts;

public interface ISupplierRatingRepository
{
    Task<Domain.SupplierRatings.SupplierRating?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.SupplierRatings.SupplierRating rating, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.SupplierRatings.SupplierRating>> ListBySupplierAsync(Guid companyId, Guid supplierId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountBySupplierAsync(Guid companyId, Guid supplierId, CancellationToken cancellationToken = default);

    /// <summary>Per-supplier average quality/delivery score and rating count, for GetSupplierScorecardReport to fold in alongside its PO-derived figures. Only suppliers with at least one rating appear — same "absent means none yet" convention as IPurchaseOrderRepository.GetSupplierOrderStatsAsync.</summary>
    Task<IReadOnlyList<(Guid SupplierId, double AverageQualityScore, double AverageDeliveryScore, int RatingCount)>> GetSupplierRatingStatsAsync(Guid companyId, CancellationToken cancellationToken = default);
}
