using FusionOS.Modules.Inventory.Domain.Ledger;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Ledger;

public class InventoryLedgerEntryTests
{
    [Fact]
    public void RecordAdjustment_WithValidData_RaisesStockAdjustedEvent()
    {
        var entry = InventoryLedgerEntry.RecordAdjustment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10m, "Cycle count correction");

        entry.QuantityDelta.Should().Be(10m);
        entry.DomainEvents.Should().ContainSingle(e => e is Events.StockAdjusted);
    }

    [Fact]
    public void RecordAdjustment_WithZeroQuantity_Throws()
    {
        var act = () => InventoryLedgerEntry.RecordAdjustment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0m, "No-op");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordAdjustment_WithoutReason_Throws()
    {
        var act = () => InventoryLedgerEntry.RecordAdjustment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5m, "");

        act.Should().Throw<ArgumentException>();
    }
}
