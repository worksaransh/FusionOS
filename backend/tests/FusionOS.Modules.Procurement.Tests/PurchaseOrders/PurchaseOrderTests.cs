using FusionOS.Modules.Procurement.Domain.PurchaseOrders;
using Events = FusionOS.Modules.Procurement.Domain.PurchaseOrders.Events;
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
    public void SubmitForApproval_FromDraft_TransitionsToPendingApprovalAndRaisesEvent()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);
        order.ClearDomainEvents();
        var requestedBy = Guid.NewGuid();

        order.SubmitForApproval(requestedBy);

        order.Status.Should().Be(PurchaseOrderStatus.PendingApproval);
        var raised = order.DomainEvents.Should().ContainSingle(e => e is Events.PurchaseOrderSubmittedForApproval)
            .Which.Should().BeOfType<Events.PurchaseOrderSubmittedForApproval>().Subject;
        raised.PurchaseOrderId.Should().Be(order.Id);
        raised.RequestedByUserId.Should().Be(requestedBy);
        raised.Amount.Should().Be(order.TotalAmount);
    }

    [Fact]
    public void SubmitForApproval_WhenNotDraft_Throws()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);
        order.Approve();

        var act = () => order.SubmitForApproval(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SubmitForApproval_WithEmptyRequestedBy_Throws()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);

        var act = () => order.SubmitForApproval(Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Approve_FromPendingApproval_TransitionsToApproved()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);
        order.SubmitForApproval(Guid.NewGuid());
        order.ClearDomainEvents();

        order.Approve();

        order.Status.Should().Be(PurchaseOrderStatus.Approved);
        order.DomainEvents.Should().ContainSingle(e => e is Events.PurchaseOrderApproved);
    }

    [Fact]
    public void Reject_FromPendingApproval_TransitionsToRejectedAndRaisesEvent()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);
        order.SubmitForApproval(Guid.NewGuid());
        order.ClearDomainEvents();

        order.Reject();

        order.Status.Should().Be(PurchaseOrderStatus.Rejected);
        order.DomainEvents.Should().ContainSingle(e => e is Events.PurchaseOrderRejected);
    }

    [Fact]
    public void Reject_FromDraft_TransitionsToRejected()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);

        order.Reject();

        order.Status.Should().Be(PurchaseOrderStatus.Rejected);
    }

    [Fact]
    public void Reject_WhenAlreadyApproved_Throws()
    {
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);
        order.Approve();

        var act = () => order.Reject();

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

    [Fact]
    public void RecordGoodsReceipt_WithUnitCost_RaisesGoodsReceiptCostedEvent()
    {
        var supplierId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var order = PurchaseOrder.Create(Guid.NewGuid(), supplierId, new[] { new PurchaseOrderLineInput(productId, 10m, 25.50m) });
        order.Approve();
        order.ClearDomainEvents();

        order.RecordGoodsReceipt(productId, 4m, 20m);

        var raised = order.DomainEvents.Should().ContainSingle(e => e is Events.PurchaseOrderGoodsReceiptCosted)
            .Which.Should().BeOfType<Events.PurchaseOrderGoodsReceiptCosted>().Subject;
        raised.SupplierId.Should().Be(supplierId);
        raised.ProductId.Should().Be(productId);
        raised.QuantityReceived.Should().Be(4m);
        raised.UnitCost.Should().Be(20m);
        raised.LineAmount.Should().Be(80m);
    }

    [Fact]
    public void RecordGoodsReceipt_WithNoUnitCost_DoesNotRaiseGoodsReceiptCostedEvent()
    {
        var productId = Guid.NewGuid();
        var order = PurchaseOrder.Create(Guid.NewGuid(), Guid.NewGuid(), new[] { new PurchaseOrderLineInput(productId, 10m, 25.50m) });
        order.Approve();
        order.ClearDomainEvents();

        order.RecordGoodsReceipt(productId, 4m);

        order.DomainEvents.Should().NotContain(e => e is Events.PurchaseOrderGoodsReceiptCosted);
    }
}
