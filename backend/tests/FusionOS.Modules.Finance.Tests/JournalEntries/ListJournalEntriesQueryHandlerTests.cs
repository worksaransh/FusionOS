using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Queries.ListJournalEntries;
using FusionOS.Modules.Finance.Domain.JournalEntries;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.JournalEntries;

public class ListJournalEntriesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedEntriesForTheCompany()
    {
        var companyId = Guid.NewGuid();
        var lines = new[]
        {
            new JournalEntryLineInput(Guid.NewGuid(), 100m, 0m, "debit"),
            new JournalEntryLineInput(Guid.NewGuid(), 0m, 100m, "credit"),
        };
        var entry = JournalEntry.Create(companyId, "REF-1", lines);
        var repository = Substitute.For<IJournalEntryRepository>();
        repository.ListAsync(companyId, 1, 25, Arg.Any<CancellationToken>()).Returns(new[] { entry });
        repository.CountAsync(companyId, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListJournalEntriesQueryHandler(repository);

        var result = await handler.Handle(new ListJournalEntriesQuery(companyId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(e => e.TotalDebit == 100m && e.Reference == "REF-1");
    }
}
