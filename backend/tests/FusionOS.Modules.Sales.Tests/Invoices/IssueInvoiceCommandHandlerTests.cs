using FusionOS.Modules.Sales.Application.Invoices.Commands.IssueInvoice;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;
using FusionOS.Modules.Sales.Domain.Invoices;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Invoices;

public class IssueInvoiceCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenInvoiceIsDraft_IssuesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var invoice = Invoice.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), new[] { new InvoiceLineInput(Guid.NewGuid(), 2m, 10m) });
        var repository = Substitute.For<IInvoiceRepository>();
        repository.GetByIdAsync(companyId, invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new IssueInvoiceCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new IssueInvoiceCommand(companyId, invoice.Id), CancellationToken.None);

        result.Status.Should().Be(nameof(InvoiceStatus.Issued));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenInvoiceIsAlreadyIssued_ThrowsInvalidOperationException()
    {
        var companyId = Guid.NewGuid();
        var invoice = Invoice.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), new[] { new InvoiceLineInput(Guid.NewGuid(), 2m, 10m) });
        var repository = Substitute.For<IInvoiceRepository>();
        repository.GetByIdAsync(companyId, invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new IssueInvoiceCommandHandler(repository, unitOfWork);
        await handler.Handle(new IssueInvoiceCommand(companyId, invoice.Id), CancellationToken.None);

        var act = () => handler.Handle(new IssueInvoiceCommand(companyId, invoice.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenInvoiceDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var repository = Substitute.For<IInvoiceRepository>();
        repository.GetByIdAsync(companyId, invoiceId, Arg.Any<CancellationToken>()).Returns((Invoice?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new IssueInvoiceCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new IssueInvoiceCommand(companyId, invoiceId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
