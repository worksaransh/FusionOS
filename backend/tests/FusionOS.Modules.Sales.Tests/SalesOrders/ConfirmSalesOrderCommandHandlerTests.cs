using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.SalesOrders.Commands.ConfirmSalesOrder;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using FusionOS.Modules.Sales.Domain.SalesOrders;
using FusionOS.SharedKernel.Context;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.SalesOrders;

public class ConfirmSalesOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOrderIsDraft_ConfirmsAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var order = SalesOrder.Create(companyId, Guid.NewGuid(), new[] { new SalesOrderLineInput(Guid.NewGuid(), 2m, 50m) });
        var repository = Substitute.For<ISalesOrderRepository>();
        repository.GetByIdAsync(companyId, order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(Guid.NewGuid());
        var sodService = Substitute.For<ISegregationOfDutiesService>();
        var handler = new ConfirmSalesOrderCommandHandler(repository, unitOfWork, currentUser, sodService);

        var result = await handler.Handle(new ConfirmSalesOrderCommand(companyId, order.Id), CancellationToken.None);

        result.Status.Should().Be(nameof(SalesOrderStatus.Confirmed));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderIsAlreadyConfirmed_ThrowsInvalidOperationException()
    {
        var companyId = Guid.NewGuid();
        var order = SalesOrder.Create(companyId, Guid.NewGuid(), new[] { new SalesOrderLineInput(Guid.NewGuid(), 2m, 50m) });
        var repository = Substitute.For<ISalesOrderRepository>();
        repository.GetByIdAsync(companyId, order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(Guid.NewGuid());
        var sodService = Substitute.For<ISegregationOfDutiesService>();
        var handler = new ConfirmSalesOrderCommandHandler(repository, unitOfWork, currentUser, sodService);
        await handler.Handle(new ConfirmSalesOrderCommand(companyId, order.Id), CancellationToken.None);

        var act = () => handler.Handle(new ConfirmSalesOrderCommand(companyId, order.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenConfirmerIsTheSameAsTheCreatorAndAnActiveSodRuleCoversTheseTwoPermissions_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var order = SalesOrder.Create(companyId, Guid.NewGuid(), new[] { new SalesOrderLineInput(Guid.NewGuid(), 2m, 50m) });
        var repository = Substitute.For<ISalesOrderRepository>();
        repository.GetByIdAsync(companyId, order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(Guid.Empty); // matches the un-stamped order.CreatedBy default
        var sodService = Substitute.For<ISegregationOfDutiesService>();
        sodService.HasConflictAsync(companyId, "sales.sales-order.create", "sales.sales-order.confirm", Arg.Any<CancellationToken>())
            .Returns(true);
        var handler = new ConfirmSalesOrderCommandHandler(repository, unitOfWork, currentUser, sodService);

        var act = () => handler.Handle(new ConfirmSalesOrderCommand(companyId, order.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var repository = Substitute.For<ISalesOrderRepository>();
        repository.GetByIdAsync(companyId, orderId, Arg.Any<CancellationToken>()).Returns((SalesOrder?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        var sodService = Substitute.For<ISegregationOfDutiesService>();
        var handler = new ConfirmSalesOrderCommandHandler(repository, unitOfWork, currentUser, sodService);

        var act = () => handler.Handle(new ConfirmSalesOrderCommand(companyId, orderId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
