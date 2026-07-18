using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.SalesOrders.Commands.CreateSalesOrder;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using FusionOS.Modules.Sales.Domain.SalesOrders;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.SalesOrders;

public class CreateSalesOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCustomerExists_PersistsOrder()
    {
        var repository = Substitute.For<ISalesOrderRepository>();
        var customerRepository = Substitute.For<ICustomerRepository>();
        customerRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateSalesOrderCommandHandler(repository, customerRepository, unitOfWork);
        var command = new CreateSalesOrderCommand(Guid.NewGuid(), Guid.NewGuid(), new[] { new SalesOrderLineInput(Guid.NewGuid(), 2m, 50m) });

        var result = await handler.Handle(command, CancellationToken.None);

        result.TotalAmount.Should().Be(100m);
        await repository.Received(1).AddAsync(Arg.Any<FusionOS.Modules.Sales.Domain.SalesOrders.SalesOrder>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCustomerDoesNotExist_Throws()
    {
        var repository = Substitute.For<ISalesOrderRepository>();
        var customerRepository = Substitute.For<ICustomerRepository>();
        customerRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateSalesOrderCommandHandler(repository, customerRepository, unitOfWork);
        var command = new CreateSalesOrderCommand(Guid.NewGuid(), Guid.NewGuid(), new[] { new SalesOrderLineInput(Guid.NewGuid(), 2m, 50m) });

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FusionOS.BuildingBlocks.Application.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task Handle_WithLineDiscountWithinThreshold_ComputesDiscountedTotal()
    {
        var repository = Substitute.For<ISalesOrderRepository>();
        var customerRepository = Substitute.For<ICustomerRepository>();
        customerRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateSalesOrderCommandHandler(repository, customerRepository, unitOfWork);
        var command = new CreateSalesOrderCommand(Guid.NewGuid(), Guid.NewGuid(), new[] { new SalesOrderLineInput(Guid.NewGuid(), 2m, 50m, 10m) });

        var result = await handler.Handle(command, CancellationToken.None);

        result.TotalAmount.Should().Be(90m);
        result.Lines.Single().DiscountPercentage.Should().Be(10m);
    }

    [Fact]
    public async Task Handle_WithLineDiscountOverThreshold_Throws()
    {
        var repository = Substitute.For<ISalesOrderRepository>();
        var customerRepository = Substitute.For<ICustomerRepository>();
        customerRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateSalesOrderCommandHandler(repository, customerRepository, unitOfWork);
        var command = new CreateSalesOrderCommand(Guid.NewGuid(), Guid.NewGuid(), new[] { new SalesOrderLineInput(Guid.NewGuid(), 2m, 50m, 25m) });

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FusionOS.BuildingBlocks.Application.Exceptions.ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<FusionOS.Modules.Sales.Domain.SalesOrders.SalesOrder>(), Arg.Any<CancellationToken>());
    }
}
