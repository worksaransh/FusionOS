using FusionOS.Modules.Warehouse.Domain.GoodsReceipts;
using FluentAssertions;
using Xunit;
using GoodsReceiptEntity = FusionOS.Modules.Warehouse.Domain.GoodsReceipts.GoodsReceipt;

namespace FusionOS.Modules.Warehouse.Tests.GoodsReceipts;

/// <summary>
/// Domain tests for Putaway (docs/IMPLEMENTATION_PLAN.md item 12), exercised
/// entirely through GoodsReceipt's public API — SuggestBin/ConfirmPutaway on
/// GoodsReceiptLine itself are internal, same reasoning PickListTests.cs used
/// for PickListLine.RecordPicked.
/// </summary>
public class GoodsReceiptPutawayTests
{
    private static GoodsReceiptEntity CreateReceiptWithOneLine(out Guid lineId)
    {
        var receipt = GoodsReceiptEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            new[] { new GoodsReceiptLineInput(Guid.NewGuid(), 10m, 5m) });

        lineId = receipt.Lines[0].Id;
        return receipt;
    }

    [Fact]
    public void SuggestBin_SetsSuggestedBinIdOnly_DoesNotAffectPutAwayBinId()
    {
        var receipt = CreateReceiptWithOneLine(out var lineId);
        var binId = Guid.NewGuid();

        receipt.SuggestBin(lineId, binId);

        receipt.Lines[0].SuggestedBinId.Should().Be(binId);
        receipt.Lines[0].PutAwayBinId.Should().BeNull();
        receipt.Lines[0].IsPutAway.Should().BeFalse();
    }

    [Fact]
    public void SuggestBin_WithUnknownLineId_Throws()
    {
        var receipt = CreateReceiptWithOneLine(out _);

        var act = () => receipt.SuggestBin(Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConfirmPutaway_SetsPutAwayBinIdAndIsPutAway()
    {
        var receipt = CreateReceiptWithOneLine(out var lineId);
        var binId = Guid.NewGuid();

        receipt.ConfirmPutaway(lineId, binId);

        receipt.Lines[0].PutAwayBinId.Should().Be(binId);
        receipt.Lines[0].IsPutAway.Should().BeTrue();
    }

    [Fact]
    public void ConfirmPutaway_DoesNotRequireAPriorSuggestion()
    {
        var receipt = CreateReceiptWithOneLine(out var lineId);

        var act = () => receipt.ConfirmPutaway(lineId, Guid.NewGuid());

        act.Should().NotThrow();
    }

    [Fact]
    public void ConfirmPutaway_CalledTwice_OverwritesWithTheSecondBin()
    {
        var receipt = CreateReceiptWithOneLine(out var lineId);
        var firstBinId = Guid.NewGuid();
        var secondBinId = Guid.NewGuid();

        receipt.ConfirmPutaway(lineId, firstBinId);
        receipt.ConfirmPutaway(lineId, secondBinId);

        receipt.Lines[0].PutAwayBinId.Should().Be(secondBinId);
    }

    [Fact]
    public void ConfirmPutaway_WithUnknownLineId_Throws()
    {
        var receipt = CreateReceiptWithOneLine(out _);

        var act = () => receipt.ConfirmPutaway(Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConfirmPutaway_RaisesGoodsReceiptLinePutAwayDomainEvent()
    {
        var receipt = CreateReceiptWithOneLine(out var lineId);
        var binId = Guid.NewGuid();

        receipt.ClearDomainEvents();
        receipt.ConfirmPutaway(lineId, binId);

        receipt.DomainEvents.Should().ContainSingle(e => e is Domain.GoodsReceipts.Events.GoodsReceiptLinePutAway putAway
            && putAway.GoodsReceiptId == receipt.Id
            && putAway.LineId == lineId
            && putAway.BinId == binId);
    }
}
