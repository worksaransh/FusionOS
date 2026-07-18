using FusionOS.Modules.Inventory.Domain.Costing;
using FusionOS.Modules.Inventory.Domain.Ledger;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Costing;

/// <summary>Covers WeightedAverageCostCalculator (M9 remaining — Inventory costing, 2026-07-16).</summary>
public class WeightedAverageCostCalculatorTests
{
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly Guid ProductId = Guid.NewGuid();
    private static readonly Guid WarehouseId = Guid.NewGuid();

    private static InventoryLedgerEntry Entry(decimal quantityDelta, decimal? unitCost, string reason = "test")
        => InventoryLedgerEntry.RecordAdjustment(CompanyId, ProductId, WarehouseId, quantityDelta, reason, unitCost);

    [Fact]
    public void Calculate_WithNoEntries_ReturnsAllZeros()
    {
        var snapshot = WeightedAverageCostCalculator.Calculate(Array.Empty<InventoryLedgerEntry>());

        snapshot.OnHandQuantity.Should().Be(0m);
        snapshot.WeightedAverageUnitCost.Should().Be(0m);
        snapshot.TotalValuation.Should().Be(0m);
        snapshot.CumulativeCostOfGoodsSold.Should().Be(0m);
    }

    [Fact]
    public void Calculate_WithSingleReceipt_SetsAverageToThatReceiptsCost()
    {
        var entries = new[] { Entry(100m, 5m) };

        var snapshot = WeightedAverageCostCalculator.Calculate(entries);

        snapshot.OnHandQuantity.Should().Be(100m);
        snapshot.WeightedAverageUnitCost.Should().Be(5m);
        snapshot.TotalValuation.Should().Be(500m);
    }

    [Fact]
    public void Calculate_WithTwoReceiptsAtDifferentCosts_BlendsWeightedAverage()
    {
        // 100 units @ 5 then 100 units @ 7 => (100*5 + 100*7) / 200 = 6
        var entries = new[] { Entry(100m, 5m), Entry(100m, 7m) };

        var snapshot = WeightedAverageCostCalculator.Calculate(entries);

        snapshot.OnHandQuantity.Should().Be(200m);
        snapshot.WeightedAverageUnitCost.Should().Be(6m);
        snapshot.TotalValuation.Should().Be(1200m);
    }

    [Fact]
    public void Calculate_WithAnIssueAfterAReceipt_UsesRunningAverageAsCogsAndLeavesAverageUnchanged()
    {
        // 100 @ 5, then issue 40 => COGS = 40*5 = 200; average stays 5; qty = 60
        var entries = new[] { Entry(100m, 5m), Entry(-40m, null) };

        var snapshot = WeightedAverageCostCalculator.Calculate(entries);

        snapshot.OnHandQuantity.Should().Be(60m);
        snapshot.WeightedAverageUnitCost.Should().Be(5m);
        snapshot.CumulativeCostOfGoodsSold.Should().Be(200m);
        snapshot.TotalValuation.Should().Be(300m);
    }

    [Fact]
    public void Calculate_WithReceiptIssueThenAnotherReceiptAtDifferentCost_BlendsFromThePostIssueQuantity()
    {
        // 100 @ 5 -> issue 40 (qty 60, avg 5) -> receive 60 @ 9
        // new average = (60*5 + 60*9) / 120 = 7
        var entries = new[] { Entry(100m, 5m), Entry(-40m, null), Entry(60m, 9m) };

        var snapshot = WeightedAverageCostCalculator.Calculate(entries);

        snapshot.OnHandQuantity.Should().Be(120m);
        snapshot.WeightedAverageUnitCost.Should().Be(7m);
        snapshot.CumulativeCostOfGoodsSold.Should().Be(200m);
    }

    [Fact]
    public void Calculate_WithReceiptCarryingNoUnitCost_TreatsItAsCostingTheCurrentRunningAverage()
    {
        // 100 @ 5 (avg=5), then receive 50 with no UnitCost (e.g. a manual adjustment with
        // no cost supplied) => blended at the current average, so the average is unchanged.
        var entries = new[] { Entry(100m, 5m), Entry(50m, null) };

        var snapshot = WeightedAverageCostCalculator.Calculate(entries);

        snapshot.OnHandQuantity.Should().Be(150m);
        snapshot.WeightedAverageUnitCost.Should().Be(5m);
    }

    [Fact]
    public void Calculate_EntriesOutOfDateOrder_AreFoldedInTransactionDateOrderRegardless()
    {
        var first = Entry(100m, 5m);
        Thread.Sleep(5); // guarantee a distinct, later TransactionDate for `second`
        var second = Entry(100m, 7m);
        // Pass in reverse order — calculator must still sort by TransactionDate internally,
        // not by input-array order, so the result must match the forward-order case above.
        var entries = new[] { second, first };

        var snapshot = WeightedAverageCostCalculator.Calculate(entries);

        snapshot.OnHandQuantity.Should().Be(200m);
        snapshot.WeightedAverageUnitCost.Should().Be(6m);
    }
}
