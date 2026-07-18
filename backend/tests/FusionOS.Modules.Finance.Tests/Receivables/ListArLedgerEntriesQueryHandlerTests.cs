using FusionOS.Modules.Finance.Application.Receivables.Contracts;
using FusionOS.Modules.Finance.Application.Receivables.Queries.ListArLedgerEntries;
using FusionOS.Modules.Finance.Domain.Receivables;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Receivables;

public class ListArLedgerEntriesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedLedgerEntriesForTheCustomer()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var entry = ArLedgerEntry.RecordInvoiceCharge(companyId, customerId, Guid.NewGuid(), 250.75m);
        var repository = Substitute.For<IArLedgerRepository>();
        repository.ListAsync(companyId, customerId, 1, 25, Arg.Any<CancellationToken>()).Returns(new[] { entry });
        repository.CountAsync(companyId, customerId, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListArLedgerEntriesQueryHandler(repository);

        var result = await handler.Handle(new ListArLedgerEntriesQuery(companyId, customerId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(e => e.Amount == 250.75m);
    }
}
