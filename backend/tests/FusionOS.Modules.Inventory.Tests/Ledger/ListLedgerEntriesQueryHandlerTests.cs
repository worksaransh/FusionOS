using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Application.Ledger.Queries.ListLedgerEntries;
using FusionOS.Modules.Inventory.Domain.Ledger;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Ledger;

public class ListLedgerEntriesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedLedgerEntriesForTheProduct()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var entry = InventoryLedgerEntry.RecordAdjustment(companyId, productId, Guid.NewGuid(), -5m, "Damaged in transit");
        var repository = Substitute.For<IInventoryLedgerRepository>();
        repository.ListAsync(companyId, productId, 1, 25, Arg.Any<CancellationToken>()).Returns(new[] { entry });
        repository.CountAsync(companyId, productId, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListLedgerEntriesQueryHandler(repository);

        var result = await handler.Handle(new ListLedgerEntriesQuery(companyId, productId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(e => e.QuantityDelta == -5m && e.Reason == "Damaged in transit");
    }
}
