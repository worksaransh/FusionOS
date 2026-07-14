using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Commands.CreateJournalEntry;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Domain.JournalEntries;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.JournalEntries;

public class CreateJournalEntryCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenEveryAccountExists_PersistsEntry()
    {
        var repository = Substitute.For<IJournalEntryRepository>();
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateJournalEntryCommandHandler(repository, accountRepository, unitOfWork);

        var lines = new[]
        {
            new JournalEntryLineInput(Guid.NewGuid(), 100m, 0m, "debit"),
            new JournalEntryLineInput(Guid.NewGuid(), 0m, 100m, "credit"),
        };
        var command = new CreateJournalEntryCommand(Guid.NewGuid(), "REF-1", lines);

        var result = await handler.Handle(command, CancellationToken.None);

        result.TotalDebit.Should().Be(100m);
        result.Status.Should().Be(nameof(JournalEntryStatus.Draft));
        await repository.Received(1).AddAsync(Arg.Any<JournalEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAnAccountDoesNotExist_Throws()
    {
        var repository = Substitute.For<IJournalEntryRepository>();
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateJournalEntryCommandHandler(repository, accountRepository, unitOfWork);

        var lines = new[]
        {
            new JournalEntryLineInput(Guid.NewGuid(), 100m, 0m, "debit"),
            new JournalEntryLineInput(Guid.NewGuid(), 0m, 100m, "credit"),
        };
        var command = new CreateJournalEntryCommand(Guid.NewGuid(), "REF-2", lines);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
