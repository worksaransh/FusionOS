using FusionOS.Modules.Inventory.Domain.Costing;
using FusionOS.Modules.Inventory.Domain.Ledger;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Costing;

/// <summary>Covers FifoCostCalculator (Phase 1 closeout, 2026-07-18 — 05_MODULE_ROADMAP.md's FIFO half of Inventory Valuation).</summary>
public class FifoCostCalculatorTests
{
    private static readonly Guid CompanyId = Guid.NewGuid();
    private static readonly Guid ProductId = Guid.NewGuid();
    private static readonly Guid WarehouseId = Guid.NewGuid();

    private static InventoryLedgerEntry Entry(decimal quantityDelta, decimal? unitCost, string reason = "test")
        => InventoryLedgerEntry.RecordAdjustment(CompanyId, ProductId, WarehouseId, quantityDelta, reason, unitCost);

    [Fact]
    public void Calculate_WithNoEntries_ReturnsAllZeros()
    {
        var snapshot = FifoCostCalculator.Calculate(Array.Empty<InventoryLedgerEntry>());

        snapshot.OnHandQuantity.Should().Be(0m);
        snapshot.CurrentUnitCost.Should().Be(0m);
        snapshot.TotalValuation.Should().Be(0m);
        snapshot.CumulativeCostOfGoodsSold.Should().Be(0m);
    }

    [Fact]
    public void Calculate_WithSingleReceipt_SetsCostToThatReceiptsCost()
    {
        var entries = new[] { Entry(100m, 5m) };

        var snapshot = FifoCostCalculator.Calculate(entries);

        snapshot.OnHandQuantity.Should().Be(100m);
        snapshot.CurrentUnitCost.Should().Be(5m);
        snapshot.TotalValuation.Should().Be(500m);
    }

    [Fact]
    public void Calculate_WithTwoLayersAtDifferentCosts_IssueConsumesOldestLayerFirst()
    {
        // 100 @ 5 then 100 @ 7. Issue 40 -> consumed entirely from the first (oldest) layer.
        // COGS = 40*5 = 200. Remaining: 60 @ 5 + 100 @ 7 = 300 + 700 = 1000 valuation, 160 qty.
        var entries = new[] { Entry(100m, 5m), Entry(100m, 7m), Entry(-40m, null) };

        var snapshot = FifoCostCalculator.Calculate(entries);

        snapshot.OnHandQuantity.Should().Be(160m);
        snapshot.CumulativeCostOfGoodsSold.Should().Be(200m);
        snapshot.TotalValuation.Should().Be(1000m);
        snapshot.CurrentUnitCost.Should().Be(6.25m);
    }

    [Fact]
    public void Calculate_WhenIssueExactlyDrainsAnEntireLayer_MovesOnToNextLayerCleanly()
    {
        // 100 @ 5 then 50 @ 8. Issue 100 -> drains the first layer exactly.
        // COGS = 100*5 = 500. Remaining: 50 @ 8 = 400 valuation.
        var entries = new[] { Entry(100m, 5m), Entry(50m, 8m), Entry(-100m, null) };

        var snapshot = FifoCostCalculator.Calculate(entries);

        snapshot.OnHandQuantity.Should().Be(50m);
        snapshot.CumulativeCostOfGoodsSold.Should().Be(500m);
        snapshot.TotalValuation.Should().Be(400m);
        snapshot.CurrentUnitCost.Should().Be(8m);
    }

    [Fact]
    public void Calculate_WhenIssueSpansMultipleLayers_ConsumesEachInOrderAtItsOwnCost()
    {
        // 100 @ 5 then 100 @ 7. Issue 150 -> all of layer 1 (100 @ 5) + 50 of layer 2 (@ 7).
        // COGS = 100*5 + 50*7 = 500 + 350 = 850. Remaining: 50 @ 7 = 350 valuation.
        var entries = new[] { Entry(100m, 5m), Entry(100m, 7m), Entry(-150m, null) };

        var snapshot = FifoCostCalculator.Calculate(entries);

        snapshot.OnHandQuantity.Should().Be(50m);
        snapshot.CumulativeCostOfGoodsSold.Should().Be(850m);
        snapshot.TotalValuation.Should().Be(350m);
        snapshot.CurrentUnitCost.Should().Be(7m);
    }

    [Fact]
    public void Calculate_DivergesFromWeightedAverage_WhenLayerCostsDiffer()
    {
        // Same entries as WeightedAverageCostCalculatorTests' blending case, but FIFO's
        // remaining-layer cost (7, since the 5-cost layer is fully issued) differs from
        // WAC's blended running average (6) — proving these are genuinely different methods.
        var entries = new[] { Entry(100m, 5m), Entry(100m, 7m), Entry(-100m, null) };

        var fifo = FifoCostCalculator.Calculate(entries);
        var wac = WeightedAverageCostCalculator.Calculate(entries);

        fifo.CurrentUnitCost.Should().Be(7m);
        wac.WeightedAverageUnitCost.Should().Be(6m);
        fifo.CurrentUnitCost.Should().NotBe(wac.WeightedAverageUnitCost);
    }

    [Fact]
    public void Calculate_WithReceiptCarryingNoUnitCost_TreatsItAsZeroCost()
    {
        // Unlike WAC (which falls back to the running average), FIFO has no single running
        // average to fall back to — a cost-less layer is simply a zero-cost layer.
        var entries = new[] { Entry(100m, null) };

        var snapshot = FifoCostCalculator.Calculate(entries);

        snapshot.OnHandQuantity.Should().Be(100m);
        snapshot.CurrentUnitCost.Should().Be(0m);
        snapshot.TotalValuation.Should().Be(0m);
    }

    [Fact]
    public void Calculate_EntriesOutOfDateOrder_AreFoldedInTransactionDateOrderRegardless()
    {
        var first = Entry(100m, 5m);
        Thread.Sleep(5); // guarantee a distinct, later TransactionDate for `second`
        var second = Entry(100m, 7m);
        // Pass in reverse order — calculator must still sort by TransactionDate internally.
        var entries = new[] { second, first, Entry(-40m, null) };

        var snapshot = FifoCostCalculator.Calculate(entries);

        snapshot.OnHandQuantity.Should().Be(160m);
        snapshot.CumulativeCostOfGoodsSold.Should().Be(200m); // issued 40 from the oldest (5-cost) layer
    }
}
