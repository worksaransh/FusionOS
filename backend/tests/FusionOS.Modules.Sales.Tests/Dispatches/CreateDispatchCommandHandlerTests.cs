using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Sales.Application.Dispatches.Commands.CreateDispatch;
using FusionOS.Modules.Sales.Application.Dispatches.Contracts;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using FusionOS.Modules.Sales.Domain.Dispatches;
using FusionOS.Modules.Sales.Domain.SalesOrders;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Dispatches;

/// <summary>
/// Covers the cross-aggregate quantity validation added in Phase M1
/// (2026-07-14 sprint), mirroring CreateInvoiceCommandHandlerTests.
/// </summary>
public class CreateDispatchCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenLineIsWithinOrderedQuantity_PersistsDispatch()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var salesOrder = SalesOrder.Create(companyId, Guid.NewGuid(), new[] { new SalesOrderLineInput(productId, 10m, 25m) });
        var repository = Substitute.For<IDispatchRepository>();
        var salesOrderRepository = Substitute.For<ISalesOrderRepository>();
        salesOrderRepository.GetByIdAsync(companyId, salesOrder.Id, Arg.Any<CancellationToken>()).Returns(salesOrder);
        repository.GetDispatchedQuantityAsync(companyId, salesOrder.Id, productId, Arg.Any<CancellationToken>()).Returns(0m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateDispatchCommandHandler(repository, salesOrderRepository, unitOfWork);
        var command = new CreateDispatchCommand(companyId, salesOrder.Id, Guid.NewGuid(), new[] { new DispatchLineInput(productId, 10m) });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Lines.Should().ContainSingle(l => l.ProductId == productId && l.QuantityDispatched == 10m);
        await repository.Received(1).AddAsync(Arg.Any<Dispatch>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLineWouldExceedRemainingOrderedQuantity_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var salesOrder = SalesOrder.Create(companyId, Guid.NewGuid(), new[] { new SalesOrderLineInput(productId, 10m, 25m) });
        var repository = Substitute.For<IDispatchRepository>();
        var salesOrderRepository = Substitute.For<ISalesOrderRepository>();
        salesOrderRepository.GetByIdAsync(companyId, salesOrder.Id, Arg.Any<CancellationToken>()).Returns(salesOrder);
        repository.GetDispatchedQuantityAsync(companyId, salesOrder.Id, productId, Arg.Any<CancellationToken>()).Returns(6m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateDispatchCommandHandler(repository, salesOrderRepository, unitOfWork);
        // 6 already dispatched + 5 requested = 11 > 10 ordered.
        var command = new CreateDispatchCommand(companyId, salesOrder.Id, Guid.NewGuid(), new[] { new DispatchLineInput(productId, 5m) });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<Dispatch>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSalesOrderDoesNotExist_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var salesOrderId = Guid.NewGuid();
        var repository = Substitute.For<IDispatchRepository>();
        var salesOrderRepository = Substitute.For<ISalesOrderRepository>();
        salesOrderRepository.GetByIdAsync(companyId, salesOrderId, Arg.Any<CancellationToken>()).Returns((SalesOrder?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateDispatchCommandHandler(repository, salesOrderRepository, unitOfWork);
        var command = new CreateDispatchCommand(companyId, salesOrderId, Guid.NewGuid(), new[] { new DispatchLineInput(Guid.NewGuid(), 1m) });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenProductIsNotPartOfTheSalesOrder_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var salesOrder = SalesOrder.Create(companyId, Guid.NewGuid(), new[] { new SalesOrderLineInput(Guid.NewGuid(), 10m, 25m) });
        var repository = Substitute.For<IDispatchRepository>();
        var salesOrderRepository = Substitute.For<ISalesOrderRepository>();
        salesOrderRepository.GetByIdAsync(companyId, salesOrder.Id, Arg.Any<CancellationToken>()).Returns(salesOrder);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateDispatchCommandHandler(repository, salesOrderRepository, unitOfWork);
        var command = new CreateDispatchCommand(companyId, salesOrder.Id, Guid.NewGuid(), new[] { new DispatchLineInput(Guid.NewGuid(), 1m) });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<Dispatch>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPartialDispatchesAccumulateToExactlyTheOrderedQuantity_PersistsDispatch()
    {
        // Prior partial dispatches (6 of 10 already dispatched) plus this request
        // (4) land exactly on the ordered quantity - at-limit must pass, not fail.
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var salesOrder = SalesOrder.Create(companyId, Guid.NewGuid(), new[] { new SalesOrderLineInput(productId, 10m, 25m) });
        var repository = Substitute.For<IDispatchRepository>();
        var salesOrderRepository = Substitute.For<ISalesOrderRepository>();
        salesOrderRepository.GetByIdAsync(companyId, salesOrder.Id, Arg.Any<CancellationToken>()).Returns(salesOrder);
        repository.GetDispatchedQuantityAsync(companyId, salesOrder.Id, productId, Arg.Any<CancellationToken>()).Returns(6m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateDispatchCommandHandler(repository, salesOrderRepository, unitOfWork);
        var command = new CreateDispatchCommand(companyId, salesOrder.Id, Guid.NewGuid(), new[] { new DispatchLineInput(productId, 4m) });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Lines.Should().ContainSingle(l => l.ProductId == productId && l.QuantityDispatched == 4m);
        await repository.Received(1).AddAsync(Arg.Any<Dispatch>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSameProductSplitAcrossRequestLinesExceedsTheCapInAggregate_ThrowsValidationException()
    {
        // Each line alone (6) fits within the ordered 10, but together they claim
        // 12 - the guard must check the request's per-product total, not each
        // line independently.
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var salesOrder = SalesOrder.Create(companyId, Guid.NewGuid(), new[] { new SalesOrderLineInput(productId, 10m, 25m) });
        var repository = Substitute.For<IDispatchRepository>();
        var salesOrderRepository = Substitute.For<ISalesOrderRepository>();
        salesOrderRepository.GetByIdAsync(companyId, salesOrder.Id, Arg.Any<CancellationToken>()).Returns(salesOrder);
        repository.GetDispatchedQuantityAsync(companyId, salesOrder.Id, productId, Arg.Any<CancellationToken>()).Returns(0m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateDispatchCommandHandler(repository, salesOrderRepository, unitOfWork);
        var command = new CreateDispatchCommand(companyId, salesOrder.Id, Guid.NewGuid(), new[]
        {
            new DispatchLineInput(productId, 6m),
            new DispatchLineInput(productId, 6m),
        });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<Dispatch>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSameProductSplitAcrossRequestLinesStaysWithinTheCap_PersistsDispatch()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var salesOrder = SalesOrder.Create(companyId, Guid.NewGuid(), new[] { new SalesOrderLineInput(productId, 10m, 25m) });
        var repository = Substitute.For<IDispatchRepository>();
        var salesOrderRepository = Substitute.For<ISalesOrderRepository>();
        salesOrderRepository.GetByIdAsync(companyId, salesOrder.Id, Arg.Any<CancellationToken>()).Returns(salesOrder);
        repository.GetDispatchedQuantityAsync(companyId, salesOrder.Id, productId, Arg.Any<CancellationToken>()).Returns(0m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateDispatchCommandHandler(repository, salesOrderRepository, unitOfWork);
        var command = new CreateDispatchCommand(companyId, salesOrder.Id, Guid.NewGuid(), new[]
        {
            new DispatchLineInput(productId, 5m),
            new DispatchLineInput(productId, 5m),
        });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Lines.Should().HaveCount(2);
        await repository.Received(1).AddAsync(Arg.Any<Dispatch>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderListsSameProductOnMultipleLines_CapIsTheSummedOrderedQuantity()
    {
        // The order carries the product on two lines (6 + 4 = 10 ordered).
        // Dispatching 10 must pass - the cap is the product's total across all
        // order lines, not just the first matching line's quantity.
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var salesOrder = SalesOrder.Create(companyId, Guid.NewGuid(), new[]
        {
            new SalesOrderLineInput(productId, 6m, 25m),
            new SalesOrderLineInput(productId, 4m, 25m),
        });
        var repository = Substitute.For<IDispatchRepository>();
        var salesOrderRepository = Substitute.For<ISalesOrderRepository>();
        salesOrderRepository.GetByIdAsync(companyId, salesOrder.Id, Arg.Any<CancellationToken>()).Returns(salesOrder);
        repository.GetDispatchedQuantityAsync(companyId, salesOrder.Id, productId, Arg.Any<CancellationToken>()).Returns(0m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateDispatchCommandHandler(repository, salesOrderRepository, unitOfWork);
        var command = new CreateDispatchCommand(companyId, salesOrder.Id, Guid.NewGuid(), new[] { new DispatchLineInput(productId, 10m) });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Lines.Should().ContainSingle(l => l.ProductId == productId && l.QuantityDispatched == 10m);
        await repository.Received(1).AddAsync(Arg.Any<Dispatch>(), Arg.Any<CancellationToken>());
    }
}
