using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Sales.Application.Invoices.Commands.CreateInvoice;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using FusionOS.Modules.Sales.Domain.Invoices;
using FusionOS.Modules.Sales.Domain.SalesOrders;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Invoices;

/// <summary>
/// Covers the cross-aggregate quantity validation added in Phase M1
/// (2026-07-14 sprint): an invoice line can never push the cumulative invoiced
/// quantity for a product past what the sales order actually ordered.
/// </summary>
public class CreateInvoiceCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenLineIsWithinOrderedQuantity_PersistsInvoice()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var salesOrder = SalesOrder.Create(companyId, Guid.NewGuid(), new[] { new SalesOrderLineInput(productId, 10m, 25m) });
        var repository = Substitute.For<IInvoiceRepository>();
        var salesOrderRepository = Substitute.For<ISalesOrderRepository>();
        salesOrderRepository.GetByIdAsync(companyId, salesOrder.Id, Arg.Any<CancellationToken>()).Returns(salesOrder);
        repository.GetInvoicedQuantityAsync(companyId, salesOrder.Id, productId, Arg.Any<CancellationToken>()).Returns(0m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateInvoiceCommandHandler(repository, salesOrderRepository, unitOfWork);
        var command = new CreateInvoiceCommand(companyId, salesOrder.Id, Guid.NewGuid(), new[] { new InvoiceLineInput(productId, 10m, 25m) });

        var result = await handler.Handle(command, CancellationToken.None);

        result.TotalAmount.Should().Be(250m);
        await repository.Received(1).AddAsync(Arg.Any<Invoice>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLineWouldExceedRemainingOrderedQuantity_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var salesOrder = SalesOrder.Create(companyId, Guid.NewGuid(), new[] { new SalesOrderLineInput(productId, 10m, 25m) });
        var repository = Substitute.For<IInvoiceRepository>();
        var salesOrderRepository = Substitute.For<ISalesOrderRepository>();
        salesOrderRepository.GetByIdAsync(companyId, salesOrder.Id, Arg.Any<CancellationToken>()).Returns(salesOrder);
        repository.GetInvoicedQuantityAsync(companyId, salesOrder.Id, productId, Arg.Any<CancellationToken>()).Returns(6m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateInvoiceCommandHandler(repository, salesOrderRepository, unitOfWork);
        // 6 already invoiced + 5 requested = 11 > 10 ordered.
        var command = new CreateInvoiceCommand(companyId, salesOrder.Id, Guid.NewGuid(), new[] { new InvoiceLineInput(productId, 5m, 25m) });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<Invoice>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductIsNotPartOfTheSalesOrder_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var salesOrder = SalesOrder.Create(companyId, Guid.NewGuid(), new[] { new SalesOrderLineInput(Guid.NewGuid(), 10m, 25m) });
        var repository = Substitute.For<IInvoiceRepository>();
        var salesOrderRepository = Substitute.For<ISalesOrderRepository>();
        salesOrderRepository.GetByIdAsync(companyId, salesOrder.Id, Arg.Any<CancellationToken>()).Returns(salesOrder);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateInvoiceCommandHandler(repository, salesOrderRepository, unitOfWork);
        var command = new CreateInvoiceCommand(companyId, salesOrder.Id, Guid.NewGuid(), new[] { new InvoiceLineInput(Guid.NewGuid(), 1m, 25m) });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenSalesOrderDoesNotExist_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var salesOrderId = Guid.NewGuid();
        var repository = Substitute.For<IInvoiceRepository>();
        var salesOrderRepository = Substitute.For<ISalesOrderRepository>();
        salesOrderRepository.GetByIdAsync(companyId, salesOrderId, Arg.Any<CancellationToken>()).Returns((SalesOrder?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateInvoiceCommandHandler(repository, salesOrderRepository, unitOfWork);
        var command = new CreateInvoiceCommand(companyId, salesOrderId, Guid.NewGuid(), new[] { new InvoiceLineInput(Guid.NewGuid(), 1m, 25m) });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenPartialInvoicesAccumulateToExactlyTheOrderedQuantity_PersistsInvoice()
    {
        // Prior partial invoices (6 of 10 already invoiced) plus this request (4)
        // land exactly on the ordered quantity - at-limit must pass, not fail.
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var salesOrder = SalesOrder.Create(companyId, Guid.NewGuid(), new[] { new SalesOrderLineInput(productId, 10m, 25m) });
        var repository = Substitute.For<IInvoiceRepository>();
        var salesOrderRepository = Substitute.For<ISalesOrderRepository>();
        salesOrderRepository.GetByIdAsync(companyId, salesOrder.Id, Arg.Any<CancellationToken>()).Returns(salesOrder);
        repository.GetInvoicedQuantityAsync(companyId, salesOrder.Id, productId, Arg.Any<CancellationToken>()).Returns(6m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateInvoiceCommandHandler(repository, salesOrderRepository, unitOfWork);
        var command = new CreateInvoiceCommand(companyId, salesOrder.Id, Guid.NewGuid(), new[] { new InvoiceLineInput(productId, 4m, 25m) });

        var result = await handler.Handle(command, CancellationToken.None);

        result.TotalAmount.Should().Be(100m);
        await repository.Received(1).AddAsync(Arg.Any<Invoice>(), Arg.Any<CancellationToken>());
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
        var repository = Substitute.For<IInvoiceRepository>();
        var salesOrderRepository = Substitute.For<ISalesOrderRepository>();
        salesOrderRepository.GetByIdAsync(companyId, salesOrder.Id, Arg.Any<CancellationToken>()).Returns(salesOrder);
        repository.GetInvoicedQuantityAsync(companyId, salesOrder.Id, productId, Arg.Any<CancellationToken>()).Returns(0m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateInvoiceCommandHandler(repository, salesOrderRepository, unitOfWork);
        var command = new CreateInvoiceCommand(companyId, salesOrder.Id, Guid.NewGuid(), new[]
        {
            new InvoiceLineInput(productId, 6m, 25m),
            new InvoiceLineInput(productId, 6m, 25m),
        });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<Invoice>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSameProductSplitAcrossRequestLinesStaysWithinTheCap_PersistsInvoice()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var salesOrder = SalesOrder.Create(companyId, Guid.NewGuid(), new[] { new SalesOrderLineInput(productId, 10m, 25m) });
        var repository = Substitute.For<IInvoiceRepository>();
        var salesOrderRepository = Substitute.For<ISalesOrderRepository>();
        salesOrderRepository.GetByIdAsync(companyId, salesOrder.Id, Arg.Any<CancellationToken>()).Returns(salesOrder);
        repository.GetInvoicedQuantityAsync(companyId, salesOrder.Id, productId, Arg.Any<CancellationToken>()).Returns(0m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateInvoiceCommandHandler(repository, salesOrderRepository, unitOfWork);
        var command = new CreateInvoiceCommand(companyId, salesOrder.Id, Guid.NewGuid(), new[]
        {
            new InvoiceLineInput(productId, 5m, 25m),
            new InvoiceLineInput(productId, 5m, 25m),
        });

        var result = await handler.Handle(command, CancellationToken.None);

        result.TotalAmount.Should().Be(250m);
        await repository.Received(1).AddAsync(Arg.Any<Invoice>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderListsSameProductOnMultipleLines_CapIsTheSummedOrderedQuantity()
    {
        // The order carries the product on two lines (6 + 4 = 10 ordered).
        // Invoicing 10 must pass - the cap is the product's total across all
        // order lines, not just the first matching line's quantity.
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var salesOrder = SalesOrder.Create(companyId, Guid.NewGuid(), new[]
        {
            new SalesOrderLineInput(productId, 6m, 25m),
            new SalesOrderLineInput(productId, 4m, 25m),
        });
        var repository = Substitute.For<IInvoiceRepository>();
        var salesOrderRepository = Substitute.For<ISalesOrderRepository>();
        salesOrderRepository.GetByIdAsync(companyId, salesOrder.Id, Arg.Any<CancellationToken>()).Returns(salesOrder);
        repository.GetInvoicedQuantityAsync(companyId, salesOrder.Id, productId, Arg.Any<CancellationToken>()).Returns(0m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateInvoiceCommandHandler(repository, salesOrderRepository, unitOfWork);
        var command = new CreateInvoiceCommand(companyId, salesOrder.Id, Guid.NewGuid(), new[] { new InvoiceLineInput(productId, 10m, 25m) });

        var result = await handler.Handle(command, CancellationToken.None);

        result.TotalAmount.Should().Be(250m);
        await repository.Received(1).AddAsync(Arg.Any<Invoice>(), Arg.Any<CancellationToken>());
    }
}
