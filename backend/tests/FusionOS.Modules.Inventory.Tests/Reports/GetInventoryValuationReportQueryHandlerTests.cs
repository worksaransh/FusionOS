using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Application.Reports.Queries.GetInventoryValuationReport;
using FusionOS.Modules.Inventory.Domain.Ledger;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Reports;

/// <summary>Covers GetInventoryValuationReportQuery (M9 remaining — Inventory costing, 2026-07-16).</summary>
public class GetInventoryValuationReportQueryHandlerTests
{
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly Guid WarehouseId = Guid.NewGuid();

    private static InventoryLedgerEntry Entry(Guid productId, decimal quantityDelta, decimal? unitCost)
        => InventoryLedgerEntry.RecordAdjustment(CompanyId, productId, WarehouseId, quantityDelta, "test", unitCost);

    [Fact]
    public async Task Handle_FoldsEachProductsEntriesThroughTheWeightedAverageCalculator()
    {
        var productId = Guid.NewGuid();
        var repository = Substitute.For<IInventoryLedgerRepository>();
        repository.GetLedgerEntriesByProductAsync(CompanyId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid, string, string, IReadOnlyList<InventoryLedgerEntry>)>
            {
                (productId, "SKU-1", "Widget", new List<InventoryLedgerEntry>
                {
                    Entry(productId, 100m, 5m),
                    Entry(productId, -40m, null),
                }),
            });
        var handler = new GetInventoryValuationReportQueryHandler(repository);

        var result = await handler.Handle(new GetInventoryValuationReportQuery(CompanyId), CancellationToken.None);

        result.Lines.Should().ContainSingle();
        var line = result.Lines[0];
        line.OnHandQuantity.Should().Be(60m);
        line.WeightedAverageUnitCost.Should().Be(5m);
        line.TotalValuation.Should().Be(300m);
        line.CumulativeCostOfGoodsSold.Should().Be(200m);
        result.GrandTotalValuation.Should().Be(300m);
        result.GrandTotalCostOfGoodsSold.Should().Be(200m);
    }

    [Fact]
    public async Task Handle_OrdersLinesBySkuAndSumsGrandTotalsAcrossProducts()
    {
        var productA = Guid.NewGuid();
        var productB = Guid.NewGuid();
        var repository = Substitute.For<IInventoryLedgerRepository>();
        repository.GetLedgerEntriesByProductAsync(CompanyId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid, string, string, IReadOnlyList<InventoryLedgerEntry>)>
            {
                (productB, "SKU-B", "Second", new List<InventoryLedgerEntry> { Entry(productB, 10m, 2m) }),
                (productA, "SKU-A", "First", new List<InventoryLedgerEntry> { Entry(productA, 5m, 3m) }),
            });
        var handler = new GetInventoryValuationReportQueryHandler(repository);

        var result = await handler.Handle(new GetInventoryValuationReportQuery(CompanyId), CancellationToken.None);

        result.Lines.Select(l => l.Sku).Should().ContainInOrder("SKU-A", "SKU-B");
        result.GrandTotalValuation.Should().Be(35m); // (5*3) + (10*2)
        result.GrandTotalCostOfGoodsSold.Should().Be(0m); // no issues in either history
    }

    [Fact]
    public async Task Handle_WithNoProducts_ReturnsEmptyReport()
    {
        var repository = Substitute.For<IInventoryLedgerRepository>();
        repository.GetLedgerEntriesByProductAsync(CompanyId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid, string, string, IReadOnlyList<InventoryLedgerEntry>)>());
        var handler = new GetInventoryValuationReportQueryHandler(repository);

        var result = await handler.Handle(new GetInventoryValuationReportQuery(CompanyId), CancellationToken.None);

        result.Lines.Should().BeEmpty();
        result.GrandTotalValuation.Should().Be(0m);
        result.GrandTotalCostOfGoodsSold.Should().Be(0m);
    }
}
