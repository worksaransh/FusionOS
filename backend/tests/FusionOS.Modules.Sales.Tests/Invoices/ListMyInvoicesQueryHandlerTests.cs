using FusionOS.Modules.Sales.Application.Invoices.Contracts;
using FusionOS.Modules.Sales.Application.Invoices.Queries.ListMyInvoices;
using FusionOS.Modules.Sales.Domain.Invoices;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Invoices;

/// <summary>Cross-customer leakage check for the portal - see ListMyQuotationsQueryHandlerTests (Quotations) for the full rationale.</summary>
public class ListMyInvoicesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsOnlyTheRequestingCustomersOwnInvoices()
    {
        var companyId = Guid.NewGuid();
        var customerA = Guid.NewGuid();
        var customerB = Guid.NewGuid();

        var invoiceForA = Invoice.Create(companyId, Guid.NewGuid(), customerA, new[] { new InvoiceLineInput(Guid.NewGuid(), 1m, 100m) });
        var invoiceForB = Invoice.Create(companyId, Guid.NewGuid(), customerB, new[] { new InvoiceLineInput(Guid.NewGuid(), 1m, 200m) });

        var repository = Substitute.For<IInvoiceRepository>();
        repository.ListByCustomerAsync(customerA, 1, 25, Arg.Any<CancellationToken>()).Returns(new[] { invoiceForA });
        repository.CountByCustomerAsync(customerA, Arg.Any<CancellationToken>()).Returns(1);
        repository.ListByCustomerAsync(customerB, 1, 25, Arg.Any<CancellationToken>()).Returns(new[] { invoiceForB });
        repository.CountByCustomerAsync(customerB, Arg.Any<CancellationToken>()).Returns(1);

        var handler = new ListMyInvoicesQueryHandler(repository);

        var resultForA = await handler.Handle(new ListMyInvoicesQuery(customerA), CancellationToken.None);
        var resultForB = await handler.Handle(new ListMyInvoicesQuery(customerB), CancellationToken.None);

        resultForA.Data.Should().ContainSingle(i => i.Id == invoiceForA.Id);
        resultForA.Data.Should().NotContain(i => i.Id == invoiceForB.Id);
        resultForB.Data.Should().ContainSingle(i => i.Id == invoiceForB.Id);
        resultForB.Data.Should().NotContain(i => i.Id == invoiceForA.Id);

        await repository.DidNotReceive().ListAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}
