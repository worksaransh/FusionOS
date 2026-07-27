using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Reports.Queries.GetSupplierScorecardReport;
using FusionOS.Modules.Procurement.Application.SupplierRatings.Contracts;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.Reports;

/// <summary>
/// Covers GetSupplierScorecardReportQuery (Phase 10 item 2, 2026-07-16) —
/// average order value and fully-received rate are computed in the handler,
/// not the repository. Extended (SupplierRating slice) to cover the
/// additive AverageQualityScore/AverageDeliveryScore/RatingCount fields,
/// which are folded in from ISupplierRatingRepository and stay null rather
/// than 0 when a supplier has no ratings recorded yet.
/// </summary>
public class GetSupplierScorecardReportQueryHandlerTests
{
    private static ISupplierRatingRepository EmptyRatingRepository(Guid companyId)
    {
        var ratingRepository = Substitute.For<ISupplierRatingRepository>();
        ratingRepository.GetSupplierRatingStatsAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid SupplierId, double AverageQualityScore, double AverageDeliveryScore, int RatingCount)>());
        return ratingRepository;
    }

    [Fact]
    public async Task Handle_ComputesAverageOrderValueAndFullyReceivedRate()
    {
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var repository = Substitute.For<IPurchaseOrderRepository>();
        repository.GetSupplierOrderStatsAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid SupplierId, int OrderCount, decimal TotalOrderValue, int FullyReceivedCount)>
            {
                (supplierId, 4, 1000m, 3),
            });
        var handler = new GetSupplierScorecardReportQueryHandler(repository, EmptyRatingRepository(companyId));

        var result = await handler.Handle(new GetSupplierScorecardReportQuery(companyId), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].SupplierId.Should().Be(supplierId);
        result[0].OrderCount.Should().Be(4);
        result[0].TotalOrderValue.Should().Be(1000m);
        result[0].AverageOrderValue.Should().Be(250m);
        result[0].FullyReceivedCount.Should().Be(3);
        result[0].FullyReceivedRate.Should().Be(75m);
    }

    [Fact]
    public async Task Handle_WithNoSuppliers_ReturnsEmptyList()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IPurchaseOrderRepository>();
        repository.GetSupplierOrderStatsAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid SupplierId, int OrderCount, decimal TotalOrderValue, int FullyReceivedCount)>());
        var handler = new GetSupplierScorecardReportQueryHandler(repository, EmptyRatingRepository(companyId));

        var result = await handler.Handle(new GetSupplierScorecardReportQuery(companyId), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithNoRatingsForSupplier_LeavesRatingFieldsNull()
    {
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var repository = Substitute.For<IPurchaseOrderRepository>();
        repository.GetSupplierOrderStatsAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid SupplierId, int OrderCount, decimal TotalOrderValue, int FullyReceivedCount)>
            {
                (supplierId, 2, 500m, 1),
            });
        var handler = new GetSupplierScorecardReportQueryHandler(repository, EmptyRatingRepository(companyId));

        var result = await handler.Handle(new GetSupplierScorecardReportQuery(companyId), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].AverageQualityScore.Should().BeNull();
        result[0].AverageDeliveryScore.Should().BeNull();
        result[0].RatingCount.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithRatingsForSupplier_FoldsInAveragesAndCount()
    {
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var repository = Substitute.For<IPurchaseOrderRepository>();
        repository.GetSupplierOrderStatsAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid SupplierId, int OrderCount, decimal TotalOrderValue, int FullyReceivedCount)>
            {
                (supplierId, 2, 500m, 1),
            });
        var ratingRepository = Substitute.For<ISupplierRatingRepository>();
        ratingRepository.GetSupplierRatingStatsAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid SupplierId, double AverageQualityScore, double AverageDeliveryScore, int RatingCount)>
            {
                (supplierId, 4.5, 3.5, 2),
            });
        var handler = new GetSupplierScorecardReportQueryHandler(repository, ratingRepository);

        var result = await handler.Handle(new GetSupplierScorecardReportQuery(companyId), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].AverageQualityScore.Should().Be(4.5);
        result[0].AverageDeliveryScore.Should().Be(3.5);
        result[0].RatingCount.Should().Be(2);
    }
}
