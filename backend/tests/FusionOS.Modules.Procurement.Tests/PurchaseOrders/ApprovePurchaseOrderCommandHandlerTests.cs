using FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.ApprovePurchaseOrder;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Domain.PurchaseOrders;
using FusionOS.SharedKernel.Context;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.PurchaseOrders;

public class ApprovePurchaseOrderCommandHandlerTests
{
    // Note: PurchaseOrder.CreatedBy is stamped by BaseDbContext.StampAudit() at
    // SaveChanges time (see AuditableAggregateRoot / TenantAggregateRoot), not by
    // the domain Create() factory used here — so in a pure handler unit test
    // (no real DbContext in the loop) a freshly-created order's CreatedBy is
    // always Guid.Empty. The self-approval guard is still exercised correctly:
    // an approver whose UserId also happens to be Guid.Empty is "the same
    // creator" from the handler's point of view, and any other UserId is not.
    [Fact]
    public async Task Handle_WhenApproverDidNotCreateTheOrder_ApprovesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var order = PurchaseOrder.Create(companyId, Guid.NewGuid(), new[] { new PurchaseOrderLineInput(Guid.NewGuid(), 5m, 10m) });
        var repository = Substitute.For<IPurchaseOrderRepository>();
        repository.GetByIdAsync(companyId, order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(Guid.NewGuid());
        var handler = new ApprovePurchaseOrderCommandHandler(repository, unitOfWork, currentUser);

        var result = await handler.Handle(new ApprovePurchaseOrderCommand(companyId, order.Id), CancellationToken.None);

        result.Status.Should().Be(nameof(PurchaseOrderStatus.Approved));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenApproverIsTheSameAsTheCreator_ThrowsInvalidOperationException()
    {
        var companyId = Guid.NewGuid();
        var order = PurchaseOrder.Create(companyId, Guid.NewGuid(), new[] { new PurchaseOrderLineInput(Guid.NewGuid(), 5m, 10m) });
        var repository = Substitute.For<IPurchaseOrderRepository>();
        repository.GetByIdAsync(companyId, order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(Guid.Empty); // matches the un-stamped order.CreatedBy default
        var handler = new ApprovePurchaseOrderCommandHandler(repository, unitOfWork, currentUser);

        var act = () => handler.Handle(new ApprovePurchaseOrderCommand(companyId, order.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderIsNotDraft_ThrowsInvalidOperationException()
    {
        var companyId = Guid.NewGuid();
        var order = PurchaseOrder.Create(companyId, Guid.NewGuid(), new[] { new PurchaseOrderLineInput(Guid.NewGuid(), 5m, 10m) });
        var repository = Substitute.For<IPurchaseOrderRepository>();
        repository.GetByIdAsync(companyId, order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(Guid.NewGuid());
        var handler = new ApprovePurchaseOrderCommandHandler(repository, unitOfWork, currentUser);
        await handler.Handle(new ApprovePurchaseOrderCommand(companyId, order.Id), CancellationToken.None);

        var act = () => handler.Handle(new ApprovePurchaseOrderCommand(companyId, order.Id), CancellationToken.None);

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
        var handler = new ApprovePurchaseOrderCommandHandler(repository, unitOfWork, currentUser);

        var act = () => handler.Handle(new ApprovePurchaseOrderCommand(companyId, orderId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
