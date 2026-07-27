using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Reports.Contracts;
using FusionOS.Modules.Procurement.Application.SupplierRatings.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.Reports.Queries.GetSupplierScorecardReport;

public sealed class GetSupplierScorecardReportQueryHandler : IRequestHandler<GetSupplierScorecardReportQuery, IReadOnlyList<SupplierScorecardLineDto>>
{
    private readonly IPurchaseOrderRepository _repository;
    private readonly ISupplierRatingRepository _ratingRepository;

    public GetSupplierScorecardReportQueryHandler(IPurchaseOrderRepository repository, ISupplierRatingRepository ratingRepository)
    {
        _repository = repository;
        _ratingRepository = ratingRepository;
    }

    public async Task<IReadOnlyList<SupplierScorecardLineDto>> Handle(GetSupplierScorecardReportQuery request, CancellationToken cancellationToken)
    {
        var stats = await _repository.GetSupplierOrderStatsAsync(request.CompanyId, cancellationToken);

        // Ratings are looked up separately and merged in memory (small,
        // one-row-per-supplier result), same "two aggregations merged by
        // SupplierId" shape PurchaseOrderRepository.GetSupplierOrderStatsAsync
        // already uses internally. A supplier with ratings but never fetched
        // here would only matter if it also had zero purchase orders — this
        // report's outer loop is (and always was) driven by PO-having
        // suppliers, so that combination simply can't appear as a row.
        var ratingStats = await _ratingRepository.GetSupplierRatingStatsAsync(request.CompanyId, cancellationToken);
        var ratingsBySupplier = ratingStats.ToDictionary(r => r.SupplierId);

        return stats
            .Select(s =>
            {
                ratingsBySupplier.TryGetValue(s.SupplierId, out var rating);
                return new SupplierScorecardLineDto(
                    s.SupplierId,
                    s.OrderCount,
                    s.TotalOrderValue,
                    s.OrderCount > 0 ? s.TotalOrderValue / s.OrderCount : 0m,
                    s.FullyReceivedCount,
                    s.OrderCount > 0 ? (decimal)s.FullyReceivedCount / s.OrderCount * 100m : 0m,
                    rating.RatingCount > 0 ? rating.AverageQualityScore : null,
                    rating.RatingCount > 0 ? rating.AverageDeliveryScore : null,
                    rating.RatingCount > 0 ? rating.RatingCount : null);
            })
            .ToList();
    }
}
