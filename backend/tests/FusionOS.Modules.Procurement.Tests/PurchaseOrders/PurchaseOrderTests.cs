using FusionOS.Modules.Procurement.Domain.PurchaseOrders;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.PurchaseOrders;

public class PurchaseOrderTests
{
    private static readonly PurchaseOrderLineInput[] OneLine =
    {
        new(Guid.NewGuid(), 10m, 25.50m),
    };

    [Fact]
    public void Create_WithValidLines_ComputesTotalAmount()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);

        order.Status.Should().Be(PurchaseOrderStatus.Draft);
        order.TotalAmount.Should().Be(255.00m);
        order.DomainEvents.Should().ContainSingle(e => e is Events.PurchaseOrderCreated);
    }

    [Fact]
    public void Create_WithNoLines_Throws()
    {
        var act = () => PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Array.Empty<PurchaseOrderLineInput>());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Approve_FromDraft_TransitionsToApprovedAndRaisesEvent()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);
        order.ClearDomainEvents();

        order.Approve();

        order.Status.Should().Be(PurchaseOrderStatus.Approved);
        order.DomainEvents.Should().ContainSingle(e => e is Events.PurchaseOrderApproved);
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_Throws()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);
        order.Approve();

        var act = () => order.Approve();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RecordGoodsReceipt_PartialQuantity_TransitionsToPartiallyReceived()
    {
        var productId = Guid.NewGuid();
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), new[] { new PurchaseOrderLineInput(productId, 10m, 25.50m) });
        order.Approve();

        order.RecordGoodsReceipt(productId, 4m);

        order.Status.Should().Be(PurchaseOrderStatus.PartiallyReceived);
        order.Lines.Single().ReceivedQuantity.Should().Be(4m);
    }

    [Fact]
    public void RecordGoodsReceipt_FullQuantity_TransitionsToFullyReceived()
    {
        var productId = Guid.NewGuid();
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), new[] { new PurchaseOrderLineInput(productId, 10m, 25.50m) });
        order.Approve();

        order.RecordGoodsReceipt(productId, 6m);
        order.RecordGoodsReceipt(productId, 4m);

        order.Status.Should().Be(PurchaseOrderStatus.FullyReceived);
    }

    [Fact]
    public void RecordGoodsReceipt_ForUnknownProduct_IsANoOp()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);
        order.Approve();

        order.RecordGoodsReceipt(Guid.NewGuid(), 5m);

        order.Status.Should().Be(PurchaseOrderStatus.Approved);
    }
}
