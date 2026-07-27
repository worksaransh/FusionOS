using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Commands.GenerateJournalEntryFromTemplate;
using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Contracts;
using FusionOS.Modules.Finance.Domain.AccountingPeriods;
using FusionOS.Modules.Finance.Domain.JournalEntries;
using FusionOS.Modules.Finance.Domain.RecurringJournalEntries;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.RecurringJournalEntries;

public class GenerateJournalEntryFromTemplateCommandHandlerTests
{
    private static (
        IRecurringJournalEntryTemplateRepository templateRepository,
        IJournalEntryRepository journalEntryRepository,
        IAccountRepository accountRepository,
        ICostCenterRepository costCenterRepository,
        IAccountingPeriodRepository accountingPeriodRepository,
        IUnitOfWork unitOfWork) Substitutes()
    {
        var templateRepository = Substitute.For<IRecurringJournalEntryTemplateRepository>();
        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var costCenterRepository = Substitute.For<ICostCenterRepository>();
        var accountingPeriodRepository = Substitute.For<IAccountingPeriodRepository>();
        accountingPeriodRepository.FindClosedPeriodCoveringDateAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns((AccountingPeriod?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        return (templateRepository, journalEntryRepository, accountRepository, costCenterRepository, accountingPeriodRepository, unitOfWork);
    }

    [Theory]
    [InlineData(RecurringJournalEntryFrequency.Monthly, 1, 0)]
    [InlineData(RecurringJournalEntryFrequency.Quarterly, 3, 0)]
    [InlineData(RecurringJournalEntryFrequency.Annual, 0, 1)]
    public async Task Handle_GeneratesDraftEntry_AndAdvancesNextDueDateByFrequency(RecurringJournalEntryFrequency frequency, int expectedMonths, int expectedYears)
    {
        var (templateRepository, journalEntryRepository, accountRepository, costCenterRepository, accountingPeriodRepository, unitOfWork) = Substitutes();
        var companyId = Guid.NewGuid();
        var nextDueDate = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
        var lines = new[]
        {
            new RecurringJournalEntryTemplateLineInput(Guid.NewGuid(), 300m, 0m),
            new RecurringJournalEntryTemplateLineInput(Guid.NewGuid(), 0m, 300m),
        };
        var template = RecurringJournalEntryTemplate.Create(companyId, "Monthly rent", frequency, nextDueDate, lines);
        templateRepository.GetByIdAsync(companyId, template.Id, Arg.Any<CancellationToken>()).Returns(template);

        var handler = new GenerateJournalEntryFromTemplateCommandHandler(templateRepository, journalEntryRepository, accountRepository, costCenterRepository, accountingPeriodRepository, unitOfWork);

        var result = await handler.Handle(new GenerateJournalEntryFromTemplateCommand(companyId, template.Id), CancellationToken.None);

        // Generated entry is a real JournalEntry, created via JournalEntry.Create — which
        // always leaves Status == Draft (matching CreateJournalEntryCommandHandler's own
        // default; neither ever calls Post()).
        result.Status.Should().Be(nameof(JournalEntryStatus.Draft));
        result.TotalDebit.Should().Be(300m);
        result.TotalCredit.Should().Be(300m);
        await journalEntryRepository.Received(1).AddAsync(Arg.Any<JournalEntry>(), Arg.Any<CancellationToken>());

        var expectedNextDueDate = nextDueDate.AddMonths(expectedMonths).AddYears(expectedYears);
        template.NextDueDate.Should().Be(expectedNextDueDate);
    }

    [Fact]
    public async Task Handle_WhenTemplateIsDeactivated_Throws()
    {
        var (templateRepository, journalEntryRepository, accountRepository, costCenterRepository, accountingPeriodRepository, unitOfWork) = Substitutes();
        var companyId = Guid.NewGuid();
        var lines = new[]
        {
            new RecurringJournalEntryTemplateLineInput(Guid.NewGuid(), 300m, 0m),
            new RecurringJournalEntryTemplateLineInput(Guid.NewGuid(), 0m, 300m),
        };
        var template = RecurringJournalEntryTemplate.Create(companyId, "Monthly rent", RecurringJournalEntryFrequency.Monthly, DateTimeOffset.UtcNow, lines);
        template.Deactivate();
        templateRepository.GetByIdAsync(companyId, template.Id, Arg.Any<CancellationToken>()).Returns(template);

        var handler = new GenerateJournalEntryFromTemplateCommandHandler(templateRepository, journalEntryRepository, accountRepository, costCenterRepository, accountingPeriodRepository, unitOfWork);

        var act = async () => await handler.Handle(new GenerateJournalEntryFromTemplateCommand(companyId, template.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await journalEntryRepository.DidNotReceive().AddAsync(Arg.Any<JournalEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPostingDateFallsInAClosedPeriod_ThrowsAndDoesNotPersistAnything()
    {
        var (templateRepository, journalEntryRepository, accountRepository, costCenterRepository, accountingPeriodRepository, unitOfWork) = Substitutes();
        var companyId = Guid.NewGuid();
        var lines = new[]
        {
            new RecurringJournalEntryTemplateLineInput(Guid.NewGuid(), 300m, 0m),
            new RecurringJournalEntryTemplateLineInput(Guid.NewGuid(), 0m, 300m),
        };
        var template = RecurringJournalEntryTemplate.Create(companyId, "Monthly rent", RecurringJournalEntryFrequency.Monthly, DateTimeOffset.UtcNow, lines);
        templateRepository.GetByIdAsync(companyId, template.Id, Arg.Any<CancellationToken>()).Returns(template);

        var closedPeriod = AccountingPeriod.Create(companyId, DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow.AddDays(30));
        closedPeriod.Close();
        accountingPeriodRepository.FindClosedPeriodCoveringDateAsync(companyId, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(closedPeriod);

        var handler = new GenerateJournalEntryFromTemplateCommandHandler(templateRepository, journalEntryRepository, accountRepository, costCenterRepository, accountingPeriodRepository, unitOfWork);

        var act = async () => await handler.Handle(new GenerateJournalEntryFromTemplateCommand(companyId, template.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await journalEntryRepository.DidNotReceive().AddAsync(Arg.Any<JournalEntry>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
