using FusionOS.Modules.Finance.Application.Payables.Contracts;
using FusionOS.Modules.Finance.Application.Payables.Queries.ListApLedgerEntries;
using FusionOS.Modules.Finance.Domain.Payables;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Payables;

public class ListApLedgerEntriesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedLedgerEntriesForTheSupplier()
    {
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var entry = ApLedgerEntry.RecordBillCharge(companyId, supplierId, Guid.NewGuid(), 250.75m, "PO bill");
        var repository = Substitute.For<IApLedgerRepository>();
        repository.ListAsync(companyId, supplierId, 1, 25, Arg.Any<CancellationToken>()).Returns(new[] { entry });
        repository.CountAsync(companyId, supplierId, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListApLedgerEntriesQueryHandler(repository);

        var result = await handler.Handle(new ListApLedgerEntriesQuery(companyId, supplierId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(e => e.Amount == 250.75m);
    }
}
