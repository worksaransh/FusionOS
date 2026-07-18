using FusionOS.Modules.Sales.Application.SalesOrders.Commands.ConfirmSalesOrder;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using FusionOS.Modules.Sales.Domain.SalesOrders;
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
        var handler = new ConfirmSalesOrderCommandHandler(repository, unitOfWork);

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
        var handler = new ConfirmSalesOrderCommandHandler(repository, unitOfWork);
        await handler.Handle(new ConfirmSalesOrderCommand(companyId, order.Id), CancellationToken.None);

        var act = () => handler.Handle(new ConfirmSalesOrderCommand(companyId, order.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var repository = Substitute.For<ISalesOrderRepository>();
        repository.GetByIdAsync(companyId, orderId, Arg.Any<CancellationToken>()).Returns((SalesOrder?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ConfirmSalesOrderCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new ConfirmSalesOrderCommand(companyId, orderId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
