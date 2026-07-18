using FusionOS.Modules.Sales.Application.Invoices.Contracts;
using FusionOS.Modules.Sales.Application.Invoices.Queries.ListInvoices;
using FusionOS.Modules.Sales.Domain.Invoices;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Invoices;

public class ListInvoicesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedInvoicesForTheCompany()
    {
        var companyId = Guid.NewGuid();
        var invoice = Invoice.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), new[] { new InvoiceLineInput(Guid.NewGuid(), 4m, 15m) });
        var repository = Substitute.For<IInvoiceRepository>();
        repository.ListAsync(companyId, 1, 25, Arg.Any<CancellationToken>()).Returns(new[] { invoice });
        repository.CountAsync(companyId, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListInvoicesQueryHandler(repository);

        var result = await handler.Handle(new ListInvoicesQuery(companyId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(i => i.TotalAmount == 60m);
    }
}
