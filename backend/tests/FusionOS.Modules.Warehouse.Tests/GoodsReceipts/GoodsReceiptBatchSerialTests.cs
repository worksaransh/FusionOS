using FusionOS.Modules.Warehouse.Domain.GoodsReceipts;
using FusionOS.Modules.Warehouse.Domain.GoodsReceipts.Events;
using FluentAssertions;
using Xunit;
using GoodsReceiptEntity = FusionOS.Modules.Warehouse.Domain.GoodsReceipts.GoodsReceipt;

namespace FusionOS.Modules.Warehouse.Tests.GoodsReceipts;

/// <summary>
/// Covers GoodsReceiptLine's BatchNumber/SerialNumber capture (M9 remaining —
/// Batch/Lot/Serial tracking, 2026-07-16) and that GoodsReceipt.Create carries
/// them through onto the raised GoodsReceiptLineReceived event, which is what
/// GoodsReceiptLineReceivedConsumer (Inventory module) reads to populate the
/// same fields on InventoryLedgerEntry.
/// </summary>
public class GoodsReceiptBatchSerialTests
{
    [Fact]
    public void Create_WithBatchAndSerialNumber_TrimsAndStoresBothOnTheLine()
    {
        var productId = Guid.NewGuid();
        var receipt = GoodsReceiptEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            new[] { new GoodsReceiptLineInput(productId, 10m, 5m, "  LOT-42  ", "  SN-001  ") });

        receipt.Lines[0].BatchNumber.Should().Be("LOT-42");
        receipt.Lines[0].SerialNumber.Should().Be("SN-001");
    }

    [Fact]
    public void Create_WithoutBatchOrSerialNumber_LeavesBothNull()
    {
        var receipt = GoodsReceiptEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            new[] { new GoodsReceiptLineInput(Guid.NewGuid(), 10m, 5m) });

        receipt.Lines[0].BatchNumber.Should().BeNull();
        receipt.Lines[0].SerialNumber.Should().BeNull();
    }

    [Fact]
    public void Create_RaisesGoodsReceiptLineReceivedCarryingBatchAndSerialNumber()
    {
        var productId = Guid.NewGuid();
        var receipt = GoodsReceiptEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            new[] { new GoodsReceiptLineInput(productId, 10m, 5m, "LOT-42", "SN-001") });

        receipt.DomainEvents.Should().ContainSingle(e => e is GoodsReceiptLineReceived received
            && received.ProductId == productId
            && received.BatchNumber == "LOT-42"
            && received.SerialNumber == "SN-001");
    }
}
