using FluentAssertions;
using FusionOS.Modules.Finance.Domain.Payables;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Payables;

public class PurchaseOrderFactTests
{
    [Fact]
    public void FromApproval_WithValidData_SetsOrderedAmountAndZeroReceived()
    {
        var fact = PurchaseOrderFact.FromApproval(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 500m);

        fact.OrderedAmount.Should().Be(500m);
        fact.ReceivedAmount.Should().Be(0m);
    }

    [Fact]
    public void FromGoodsReceipt_WithValidData_LeavesOrderedAmountNull()
    {
        var fact = PurchaseOrderFact.FromGoodsReceipt(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 300m);

        fact.OrderedAmount.Should().BeNull();
        fact.ReceivedAmount.Should().Be(300m);
    }

    [Fact]
    public void FromApproval_WithEmptyPurchaseOrderId_Throws()
    {
        var act = () => PurchaseOrderFact.FromApproval(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), 500m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromApproval_WithNegativeOrderedAmount_Throws()
    {
        var act = () => PurchaseOrderFact.FromApproval(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), -1m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ApplyApproval_WhenGoodsReceiptCreatedTheFactFirst_FillsInTheOrderedAmount()
    {
        var fact = PurchaseOrderFact.FromGoodsReceipt(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 300m);

        fact.ApplyApproval(1000m);

        fact.OrderedAmount.Should().Be(1000m);
        fact.ReceivedAmount.Should().Be(300m);
    }

    [Fact]
    public void ApplyGoodsReceipt_AccumulatesIntoTheRunningReceivedTotal()
    {
        var fact = PurchaseOrderFact.FromApproval(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1000m);

        fact.ApplyGoodsReceipt(300m);
        fact.ApplyGoodsReceipt(200m);

        fact.ReceivedAmount.Should().Be(500m);
        fact.OrderedAmount.Should().Be(1000m);
    }

    [Fact]
    public void ApplyGoodsReceipt_WithNegativeAmount_Throws()
    {
        var fact = PurchaseOrderFact.FromApproval(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1000m);

        var act = () => fact.ApplyGoodsReceipt(-1m);

        act.Should().Throw<ArgumentException>();
    }
}
