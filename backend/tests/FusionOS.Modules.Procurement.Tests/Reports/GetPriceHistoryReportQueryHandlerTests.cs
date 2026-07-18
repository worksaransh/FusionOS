using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Reports.Queries.GetPriceHistoryReport;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.Reports;

/// <summary>Covers GetPriceHistoryReportQuery (Phase 1 closeout, 2026-07-18) — a thin pass-through over IPurchaseOrderRepository.GetPriceHistoryAsync, mapping tuples to DTOs.</summary>
public class GetPriceHistoryReportQueryHandlerTests
{
    [Fact]
    public async Task Handle_MapsRepositoryTuplesToDtos()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var purchaseOrderId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var orderDate = DateTimeOffset.UtcNow.AddDays(-10);
        var repository = Substitute.For<IPurchaseOrderRepository>();
        repository.GetPriceHistoryAsync(companyId, productId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid PurchaseOrderId, Guid SupplierId, DateTimeOffset OrderDate, decimal UnitPrice, decimal Quantity)>
            {
                (purchaseOrderId, supplierId, orderDate, 12.5m, 100m),
            });
        var handler = new GetPriceHistoryReportQueryHandler(repository);

        var result = await handler.Handle(new GetPriceHistoryReportQuery(companyId, productId), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].PurchaseOrderId.Should().Be(purchaseOrderId);
        result[0].SupplierId.Should().Be(supplierId);
        result[0].OrderDate.Should().Be(orderDate);
        result[0].UnitPrice.Should().Be(12.5m);
        result[0].Quantity.Should().Be(100m);
    }

    [Fact]
    public async Task Handle_WithNoHistory_ReturnsEmptyList()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var repository = Substitute.For<IPurchaseOrderRepository>();
        repository.GetPriceHistoryAsync(companyId, productId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid PurchaseOrderId, Guid SupplierId, DateTimeOffset OrderDate, decimal UnitPrice, decimal Quantity)>());
        var handler = new GetPriceHistoryReportQueryHandler(repository);

        var result = await handler.Handle(new GetPriceHistoryReportQuery(companyId, productId), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
