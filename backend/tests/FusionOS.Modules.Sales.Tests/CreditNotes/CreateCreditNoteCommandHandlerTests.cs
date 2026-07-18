using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Sales.Application.CreditNotes.Commands.CreateCreditNote;
using FusionOS.Modules.Sales.Application.CreditNotes.Contracts;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;
using FusionOS.Modules.Sales.Domain.CreditNotes;
using FusionOS.Modules.Sales.Domain.Invoices;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.CreditNotes;

/// <summary>
/// Covers the cross-aggregate quantity validation mirroring
/// CreateInvoiceCommandHandlerTests (Phase M1): a credit note line can never
/// push the cumulative credited quantity for a product past what the invoice
/// actually billed.
/// </summary>
public class CreateCreditNoteCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenLineIsWithinInvoicedQuantity_PersistsCreditNote()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var invoice = Invoice.Create(companyId, Guid.NewGuid(), customerId, new[] { new InvoiceLineInput(productId, 10m, 25m) });
        var repository = Substitute.For<ICreditNoteRepository>();
        var invoiceRepository = Substitute.For<IInvoiceRepository>();
        invoiceRepository.GetByIdAsync(companyId, invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        repository.GetCreditedQuantityAsync(companyId, invoice.Id, productId, Arg.Any<CancellationToken>()).Returns(0m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateCreditNoteCommandHandler(repository, invoiceRepository, unitOfWork);
        var command = new CreateCreditNoteCommand(companyId, invoice.Id, customerId, "Damaged goods", new[] { new CreditNoteLineInput(productId, 10m, 25m) });

        var result = await handler.Handle(command, CancellationToken.None);

        result.TotalAmount.Should().Be(250m);
        await repository.Received(1).AddAsync(Arg.Any<CreditNote>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLineWouldExceedRemainingInvoicedQuantity_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var invoice = Invoice.Create(companyId, Guid.NewGuid(), customerId, new[] { new InvoiceLineInput(productId, 10m, 25m) });
        var repository = Substitute.For<ICreditNoteRepository>();
        var invoiceRepository = Substitute.For<IInvoiceRepository>();
        invoiceRepository.GetByIdAsync(companyId, invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        repository.GetCreditedQuantityAsync(companyId, invoice.Id, productId, Arg.Any<CancellationToken>()).Returns(6m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateCreditNoteCommandHandler(repository, invoiceRepository, unitOfWork);
        // 6 already credited + 5 requested = 11 > 10 invoiced.
        var command = new CreateCreditNoteCommand(companyId, invoice.Id, customerId, "Damaged goods", new[] { new CreditNoteLineInput(productId, 5m, 25m) });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<CreditNote>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductIsNotPartOfTheInvoice_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var invoice = Invoice.Create(companyId, Guid.NewGuid(), customerId, new[] { new InvoiceLineInput(Guid.NewGuid(), 10m, 25m) });
        var repository = Substitute.For<ICreditNoteRepository>();
        var invoiceRepository = Substitute.For<IInvoiceRepository>();
        invoiceRepository.GetByIdAsync(companyId, invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateCreditNoteCommandHandler(repository, invoiceRepository, unitOfWork);
        var command = new CreateCreditNoteCommand(companyId, invoice.Id, customerId, "Damaged goods", new[] { new CreditNoteLineInput(Guid.NewGuid(), 1m, 25m) });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenInvoiceDoesNotExist_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var repository = Substitute.For<ICreditNoteRepository>();
        var invoiceRepository = Substitute.For<IInvoiceRepository>();
        invoiceRepository.GetByIdAsync(companyId, invoiceId, Arg.Any<CancellationToken>()).Returns((Invoice?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateCreditNoteCommandHandler(repository, invoiceRepository, unitOfWork);
        var command = new CreateCreditNoteCommand(companyId, invoiceId, Guid.NewGuid(), "Damaged goods", new[] { new CreditNoteLineInput(Guid.NewGuid(), 1m, 25m) });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenCustomerDoesNotMatchTheInvoicesCustomer_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var invoice = Invoice.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), new[] { new InvoiceLineInput(Guid.NewGuid(), 10m, 25m) });
        var repository = Substitute.For<ICreditNoteRepository>();
        var invoiceRepository = Substitute.For<IInvoiceRepository>();
        invoiceRepository.GetByIdAsync(companyId, invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateCreditNoteCommandHandler(repository, invoiceRepository, unitOfWork);
        var command = new CreateCreditNoteCommand(companyId, invoice.Id, Guid.NewGuid(), "Damaged goods", new[] { new CreditNoteLineInput(Guid.NewGuid(), 1m, 25m) });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
