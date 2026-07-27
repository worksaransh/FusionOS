using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Commands.CreateJournalEntry;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Domain.AccountingPeriods;
using FusionOS.Modules.Finance.Domain.JournalEntries;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.JournalEntries;

public class CreateJournalEntryCommandHandlerTests
{
    private static (IJournalEntryRepository repository, IAccountRepository accountRepository, ICostCenterRepository costCenterRepository, IAccountingPeriodRepository accountingPeriodRepository, IUnitOfWork unitOfWork) Substitutes()
    {
        var repository = Substitute.For<IJournalEntryRepository>();
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var costCenterRepository = Substitute.For<ICostCenterRepository>();
        var accountingPeriodRepository = Substitute.For<IAccountingPeriodRepository>();
        // No closed period covers "now" unless a test explicitly overrides this — the
        // regression case for "no period defined at all" relies on this exact default.
        accountingPeriodRepository.FindClosedPeriodCoveringDateAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns((AccountingPeriod?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        return (repository, accountRepository, costCenterRepository, accountingPeriodRepository, unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenEveryAccountExists_PersistsEntry()
    {
        var (repository, accountRepository, costCenterRepository, accountingPeriodRepository, unitOfWork) = Substitutes();
        var handler = new CreateJournalEntryCommandHandler(repository, accountRepository, costCenterRepository, accountingPeriodRepository, unitOfWork);

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
        var (repository, accountRepository, costCenterRepository, accountingPeriodRepository, unitOfWork) = Substitutes();
        accountRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var handler = new CreateJournalEntryCommandHandler(repository, accountRepository, costCenterRepository, accountingPeriodRepository, unitOfWork);

        var lines = new[]
        {
            new JournalEntryLineInput(Guid.NewGuid(), 100m, 0m, "debit"),
            new JournalEntryLineInput(Guid.NewGuid(), 0m, 100m, "credit"),
        };
        var command = new CreateJournalEntryCommand(Guid.NewGuid(), "REF-2", lines);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenPostingDateFallsInAClosedPeriod_Throws()
    {
        var (repository, accountRepository, costCenterRepository, accountingPeriodRepository, unitOfWork) = Substitutes();
        var companyId = Guid.NewGuid();
        var closedPeriod = AccountingPeriod.Create(companyId, DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow.AddDays(30));
        closedPeriod.Close();
        accountingPeriodRepository.FindClosedPeriodCoveringDateAsync(companyId, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(closedPeriod);
        var handler = new CreateJournalEntryCommandHandler(repository, accountRepository, costCenterRepository, accountingPeriodRepository, unitOfWork);

        var lines = new[]
        {
            new JournalEntryLineInput(Guid.NewGuid(), 100m, 0m, "debit"),
            new JournalEntryLineInput(Guid.NewGuid(), 0m, 100m, "credit"),
        };
        var command = new CreateJournalEntryCommand(companyId, "REF-CLOSED", lines);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<JournalEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPostingDateFallsInAnOpenPeriod_PersistsEntry()
    {
        var (repository, accountRepository, costCenterRepository, accountingPeriodRepository, unitOfWork) = Substitutes();
        var companyId = Guid.NewGuid();
        // An Open period covering "now" exists, but FindClosedPeriodCoveringDateAsync only ever
        // returns a period whose Status is Closed (see IAccountingPeriodRepository's own doc
        // comment) — an Open period covering the date is never returned, so this stays null,
        // exactly like the "no period defined" case below.
        accountingPeriodRepository.FindClosedPeriodCoveringDateAsync(companyId, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns((AccountingPeriod?)null);
        var handler = new CreateJournalEntryCommandHandler(repository, accountRepository, costCenterRepository, accountingPeriodRepository, unitOfWork);

        var lines = new[]
        {
            new JournalEntryLineInput(Guid.NewGuid(), 100m, 0m, "debit"),
            new JournalEntryLineInput(Guid.NewGuid(), 0m, 100m, "credit"),
        };
        var command = new CreateJournalEntryCommand(companyId, "REF-OPEN", lines);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(nameof(JournalEntryStatus.Draft));
        await repository.Received(1).AddAsync(Arg.Any<JournalEntry>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Regression test: a company that has never created any AccountingPeriod at all must be
    /// completely unaffected by this feature — posting behaves exactly as it did before period
    /// control existed. FindClosedPeriodCoveringDateAsync returning null covers both "no period
    /// exists at all" and "a period exists but isn't Closed"; this test is the "doesn't exist at
    /// all" half of that contract.
    /// </summary>
    [Fact]
    public async Task Handle_WhenNoAccountingPeriodIsDefinedAtAll_PersistsEntry()
    {
        var (repository, accountRepository, costCenterRepository, accountingPeriodRepository, unitOfWork) = Substitutes();
        var handler = new CreateJournalEntryCommandHandler(repository, accountRepository, costCenterRepository, accountingPeriodRepository, unitOfWork);

        var lines = new[]
        {
            new JournalEntryLineInput(Guid.NewGuid(), 250m, 0m, "debit"),
            new JournalEntryLineInput(Guid.NewGuid(), 0m, 250m, "credit"),
        };
        var command = new CreateJournalEntryCommand(Guid.NewGuid(), "REF-NOPERIOD", lines);

        var result = await handler.Handle(command, CancellationToken.None);

        result.TotalDebit.Should().Be(250m);
        await repository.Received(1).AddAsync(Arg.Any<JournalEntry>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
