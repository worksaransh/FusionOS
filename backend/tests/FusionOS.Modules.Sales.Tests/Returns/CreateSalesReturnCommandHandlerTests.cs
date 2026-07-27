using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;
using FusionOS.Modules.Sales.Application.Returns.Commands.CreateSalesReturn;
using FusionOS.Modules.Sales.Application.Returns.Contracts;
using FusionOS.Modules.Sales.Domain.Invoices;
using FusionOS.Modules.Sales.Domain.Returns;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Returns;

/// <summary>
/// Covers the cross-aggregate quantity validation mirroring
/// CreateCreditNoteCommandHandlerTests (which itself mirrors
/// CreateInvoiceCommandHandlerTests): a sales return line can never push the
/// cumulative returned quantity for a product past what the invoice actually billed.
/// </summary>
public class CreateSalesReturnCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenLineIsWithinInvoicedQuantity_PersistsSalesReturn()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var invoice = Invoice.Create(companyId, Guid.NewGuid(), customerId, new[] { new InvoiceLineInput(productId, 10m, 25m) });
        var repository = Substitute.For<ISalesReturnRepository>();
        var invoiceRepository = Substitute.For<IInvoiceRepository>();
        invoiceRepository.GetByIdAsync(companyId, invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        repository.GetReturnedQuantityAsync(companyId, invoice.Id, productId, Arg.Any<CancellationToken>()).Returns(0m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateSalesReturnCommandHandler(repository, invoiceRepository, unitOfWork);
        var command = new CreateSalesReturnCommand(companyId, invoice.Id, customerId, "Damaged goods", new[] { new SalesReturnLineInput(productId, 10m) });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(nameof(SalesReturnStatus.Requested));
        await repository.Received(1).AddAsync(Arg.Any<SalesReturn>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSingleLineExceedsRemainingReturnableQuantity_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var invoice = Invoice.Create(companyId, Guid.NewGuid(), customerId, new[] { new InvoiceLineInput(productId, 10m, 25m) });
        var repository = Substitute.For<ISalesReturnRepository>();
        var invoiceRepository = Substitute.For<IInvoiceRepository>();
        invoiceRepository.GetByIdAsync(companyId, invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        repository.GetReturnedQuantityAsync(companyId, invoice.Id, productId, Arg.Any<CancellationToken>()).Returns(6m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateSalesReturnCommandHandler(repository, invoiceRepository, unitOfWork);
        // 6 already returned + 5 requested = 11 > 10 invoiced.
        var command = new CreateSalesReturnCommand(companyId, invoice.Id, customerId, "Damaged goods", new[] { new SalesReturnLineInput(productId, 5m) });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<SalesReturn>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSameProductSplitAcrossRequestLinesExceedsInvoicedQuantity_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var invoice = Invoice.Create(companyId, Guid.NewGuid(), customerId, new[] { new InvoiceLineInput(productId, 10m, 25m) });
        var repository = Substitute.For<ISalesReturnRepository>();
        var invoiceRepository = Substitute.For<IInvoiceRepository>();
        invoiceRepository.GetByIdAsync(companyId, invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        repository.GetReturnedQuantityAsync(companyId, invoice.Id, productId, Arg.Any<CancellationToken>()).Returns(0m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateSalesReturnCommandHandler(repository, invoiceRepository, unitOfWork);
        // Each line alone (6) is within the invoiced 10, but together they return 12.
        var command = new CreateSalesReturnCommand(companyId, invoice.Id, customerId, "Damaged goods", new[]
        {
            new SalesReturnLineInput(productId, 6m),
            new SalesReturnLineInput(productId, 6m),
        });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<SalesReturn>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCumulativeAcrossMultipleExistingReturnsExceedsInvoicedQuantity_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var invoice = Invoice.Create(companyId, Guid.NewGuid(), customerId, new[] { new InvoiceLineInput(productId, 10m, 25m) });
        var repository = Substitute.For<ISalesReturnRepository>();
        var invoiceRepository = Substitute.For<IInvoiceRepository>();
        invoiceRepository.GetByIdAsync(companyId, invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        // Two earlier sales returns (e.g. 4 + 4) already sum to 8 returned against this invoice.
        repository.GetReturnedQuantityAsync(companyId, invoice.Id, productId, Arg.Any<CancellationToken>()).Returns(8m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateSalesReturnCommandHandler(repository, invoiceRepository, unitOfWork);
        // 8 already returned + 5 requested = 13 > 10 invoiced.
        var command = new CreateSalesReturnCommand(companyId, invoice.Id, customerId, "Damaged goods", new[] { new SalesReturnLineInput(productId, 5m) });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<SalesReturn>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductIsNotPartOfTheInvoice_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var invoice = Invoice.Create(companyId, Guid.NewGuid(), customerId, new[] { new InvoiceLineInput(Guid.NewGuid(), 10m, 25m) });
        var repository = Substitute.For<ISalesReturnRepository>();
        var invoiceRepository = Substitute.For<IInvoiceRepository>();
        invoiceRepository.GetByIdAsync(companyId, invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateSalesReturnCommandHandler(repository, invoiceRepository, unitOfWork);
        var command = new CreateSalesReturnCommand(companyId, invoice.Id, customerId, "Damaged goods", new[] { new SalesReturnLineInput(Guid.NewGuid(), 1m) });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenInvoiceDoesNotExist_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var repository = Substitute.For<ISalesReturnRepository>();
        var invoiceRepository = Substitute.For<IInvoiceRepository>();
        invoiceRepository.GetByIdAsync(companyId, invoiceId, Arg.Any<CancellationToken>()).Returns((Invoice?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateSalesReturnCommandHandler(repository, invoiceRepository, unitOfWork);
        var command = new CreateSalesReturnCommand(companyId, invoiceId, Guid.NewGuid(), "Damaged goods", new[] { new SalesReturnLineInput(Guid.NewGuid(), 1m) });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenCustomerDoesNotMatchTheInvoicesCustomer_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var invoice = Invoice.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), new[] { new InvoiceLineInput(Guid.NewGuid(), 10m, 25m) });
        var repository = Substitute.For<ISalesReturnRepository>();
        var invoiceRepository = Substitute.For<IInvoiceRepository>();
        invoiceRepository.GetByIdAsync(companyId, invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateSalesReturnCommandHandler(repository, invoiceRepository, unitOfWork);
        var command = new CreateSalesReturnCommand(companyId, invoice.Id, Guid.NewGuid(), "Damaged goods", new[] { new SalesReturnLineInput(Guid.NewGuid(), 1m) });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
