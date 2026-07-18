using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Application.Reports.Queries.GetStockValuationReport;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Reports;

/// <summary>Covers GetStockValuationReportQuery (Phase M6, 2026-07-15) — ExtendedValue math and the null-cost edge case.</summary>
public class GetStockValuationReportQueryHandlerTests
{
    [Fact]
    public async Task Handle_ComputesExtendedValueAsOnHandQuantityTimesLastUnitCost()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var repository = Substitute.For<IInventoryLedgerRepository>();
        repository.GetStockValuationAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid, string, string, decimal, decimal?)>
            {
                (productId, "SKU-1", "Widget", 10m, 2.5m),
            });
        var handler = new GetStockValuationReportQueryHandler(repository);

        var result = await handler.Handle(new GetStockValuationReportQuery(companyId), CancellationToken.None);

        result.Lines.Should().ContainSingle();
        result.Lines[0].ExtendedValue.Should().Be(25m);
        result.GrandTotalValue.Should().Be(25m);
    }

    [Fact]
    public async Task Handle_WhenLastUnitCostIsNull_TreatsItAsZeroForValuation()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var repository = Substitute.For<IInventoryLedgerRepository>();
        repository.GetStockValuationAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid, string, string, decimal, decimal?)>
            {
                (productId, "SKU-2", "Gadget", 5m, null),
            });
        var handler = new GetStockValuationReportQueryHandler(repository);

        var result = await handler.Handle(new GetStockValuationReportQuery(companyId), CancellationToken.None);

        result.Lines[0].ExtendedValue.Should().Be(0m);
        result.Lines[0].LastUnitCost.Should().BeNull();
    }

    [Fact]
    public async Task Handle_OrdersLinesBySkuAndSumsGrandTotal()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IInventoryLedgerRepository>();
        repository.GetStockValuationAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid, string, string, decimal, decimal?)>
            {
                (Guid.NewGuid(), "SKU-B", "Second", 2m, 10m),
                (Guid.NewGuid(), "SKU-A", "First", 3m, 5m),
            });
        var handler = new GetStockValuationReportQueryHandler(repository);

        var result = await handler.Handle(new GetStockValuationReportQuery(companyId), CancellationToken.None);

        result.Lines.Select(l => l.Sku).Should().ContainInOrder("SKU-A", "SKU-B");
        result.GrandTotalValue.Should().Be(35m); // (3*5) + (2*10)
    }
}
