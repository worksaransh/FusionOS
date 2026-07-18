using FluentAssertions;
using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;
using FusionOS.Modules.Finance.Application.BankStatementLines.Queries.SuggestMatchesForStatementLine;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Domain.BankStatementLines;
using FusionOS.Modules.Finance.Domain.JournalEntries;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.BankStatementLines;

public class SuggestMatchesForStatementLineQueryHandlerTests
{
    private static JournalEntry PostedEntryOf(decimal amount)
    {
        var lines = new[]
        {
            new JournalEntryLineInput(Guid.NewGuid(), amount, 0m, "dr"),
            new JournalEntryLineInput(Guid.NewGuid(), 0m, amount, "cr"),
        };
        var entry = JournalEntry.Create(Guid.NewGuid(), "REF", lines);
        entry.Post();
        return entry;
    }

    [Fact]
    public async Task Handle_ReturnsSameAmountCandidatesAndExcludesAlreadyMatched()
    {
        var companyId = Guid.NewGuid();
        var line = BankStatementLine.Create(companyId, Guid.NewGuid(), DateTimeOffset.UtcNow, 100m, "ACME deposit");
        var candidateA = PostedEntryOf(100m);
        var candidateB = PostedEntryOf(100m);

        var statementLineRepository = Substitute.For<IBankStatementLineRepository>();
        statementLineRepository.GetByIdAsync(companyId, line.Id, Arg.Any<CancellationToken>()).Returns(line);
        statementLineRepository.GetMatchedJournalEntryIdsAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new[] { candidateB.Id });

        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        journalEntryRepository.FindPostedByAmountWithinDateRangeAsync(companyId, 100m, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new[] { candidateA, candidateB });

        var handler = new SuggestMatchesForStatementLineQueryHandler(statementLineRepository, journalEntryRepository);

        var result = await handler.Handle(new SuggestMatchesForStatementLineQuery(companyId, line.Id), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].JournalEntryId.Should().Be(candidateA.Id);
        result[0].Amount.Should().Be(100m);
    }

    [Fact]
    public async Task Handle_UsesAbsoluteAmountForWithdrawals()
    {
        var companyId = Guid.NewGuid();
        var line = BankStatementLine.Create(companyId, Guid.NewGuid(), DateTimeOffset.UtcNow, -250m, "Rent withdrawal");

        var statementLineRepository = Substitute.For<IBankStatementLineRepository>();
        statementLineRepository.GetByIdAsync(companyId, line.Id, Arg.Any<CancellationToken>()).Returns(line);
        statementLineRepository.GetMatchedJournalEntryIdsAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Guid>());

        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        journalEntryRepository.FindPostedByAmountWithinDateRangeAsync(companyId, 250m, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new[] { PostedEntryOf(250m) });

        var handler = new SuggestMatchesForStatementLineQueryHandler(statementLineRepository, journalEntryRepository);

        var result = await handler.Handle(new SuggestMatchesForStatementLineQuery(companyId, line.Id), CancellationToken.None);

        result.Should().ContainSingle();
        // Verify the query was asked for the absolute magnitude (250), not -250.
        await journalEntryRepository.Received(1)
            .FindPostedByAmountWithinDateRangeAsync(companyId, 250m, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStatementLineDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var statementLineRepository = Substitute.For<IBankStatementLineRepository>();
        statementLineRepository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((BankStatementLine?)null);
        var handler = new SuggestMatchesForStatementLineQueryHandler(statementLineRepository, Substitute.For<IJournalEntryRepository>());

        var act = () => handler.Handle(new SuggestMatchesForStatementLineQuery(companyId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
