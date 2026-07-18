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

    [Fact]
    public void RecordAdjustment_WithBatchAndSerialNumber_TrimsAndStoresBoth()
    {
        var entry = InventoryLedgerEntry.RecordAdjustment(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10m, "Goods receipt", 5m, "  LOT-42  ", "  SN-001  ");

        entry.BatchNumber.Should().Be("LOT-42");
        entry.SerialNumber.Should().Be("SN-001");
    }

    [Fact]
    public void RecordAdjustment_WithoutBatchOrSerialNumber_LeavesBothNull()
    {
        var entry = InventoryLedgerEntry.RecordAdjustment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10m, "Goods receipt");

        entry.BatchNumber.Should().BeNull();
        entry.SerialNumber.Should().BeNull();
    }

    [Fact]
    public void RecordAdjustment_WithWhitespaceOnlyBatchNumber_TreatsItAsNull()
    {
        var entry = InventoryLedgerEntry.RecordAdjustment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10m, "Goods receipt", 5m, "   ");

        entry.BatchNumber.Should().BeNull();
    }
}
