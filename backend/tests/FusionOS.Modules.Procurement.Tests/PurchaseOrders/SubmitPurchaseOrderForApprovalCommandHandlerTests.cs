using FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.SubmitPurchaseOrderForApproval;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.Modules.Procurement.Domain.PurchaseOrders;
using FusionOS.SharedKernel.Context;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.PurchaseOrders;

public class SubmitPurchaseOrderForApprovalCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOrderIsDraft_SubmitsForApprovalAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var order = PurchaseOrder.Create(companyId, Guid.NewGuid(), new[] { new PurchaseOrderLineInput(Guid.NewGuid(), 5m, 10m) });
        var repository = Substitute.For<IPurchaseOrderRepository>();
        repository.GetByIdAsync(companyId, order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(Guid.NewGuid());
        var handler = new SubmitPurchaseOrderForApprovalCommandHandler(repository, unitOfWork, currentUser);

        var result = await handler.Handle(new SubmitPurchaseOrderForApprovalCommand(companyId, order.Id), CancellationToken.None);

        result.Status.Should().Be(nameof(PurchaseOrderStatus.PendingApproval));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoAuthenticatedUser_Throws()
    {
        var companyId = Guid.NewGuid();
        var order = PurchaseOrder.Create(companyId, Guid.NewGuid(), new[] { new PurchaseOrderLineInput(Guid.NewGuid(), 5m, 10m) });
        var repository = Substitute.For<IPurchaseOrderRepository>();
        repository.GetByIdAsync(companyId, order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns((Guid?)null);
        var handler = new SubmitPurchaseOrderForApprovalCommandHandler(repository, unitOfWork, currentUser);

        var act = () => handler.Handle(new SubmitPurchaseOrderForApprovalCommand(companyId, order.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderIsNotDraft_ThrowsInvalidOperationException()
    {
        var companyId = Guid.NewGuid();
        var order = PurchaseOrder.Create(companyId, Guid.NewGuid(), new[] { new PurchaseOrderLineInput(Guid.NewGuid(), 5m, 10m) });
        order.Approve();
        var repository = Substitute.For<IPurchaseOrderRepository>();
        repository.GetByIdAsync(companyId, order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(Guid.NewGuid());
        var handler = new SubmitPurchaseOrderForApprovalCommandHandler(repository, unitOfWork, currentUser);

        var act = () => handler.Handle(new SubmitPurchaseOrderForApprovalCommand(companyId, order.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var repository = Substitute.For<IPurchaseOrderRepository>();
        repository.GetByIdAsync(companyId, orderId, Arg.Any<CancellationToken>()).Returns((PurchaseOrder?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        var handler = new SubmitPurchaseOrderForApprovalCommandHandler(repository, unitOfWork, currentUser);

        var act = () => handler.Handle(new SubmitPurchaseOrderForApprovalCommand(companyId, orderId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
