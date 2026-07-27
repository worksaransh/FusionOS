using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Commands.CreateRecurringJournalEntryTemplate;
using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Contracts;
using FusionOS.Modules.Finance.Domain.RecurringJournalEntries;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.RecurringJournalEntries;

public class CreateRecurringJournalEntryTemplateCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenEveryAccountExists_PersistsTemplate()
    {
        var repository = Substitute.For<IRecurringJournalEntryTemplateRepository>();
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var costCenterRepository = Substitute.For<ICostCenterRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateRecurringJournalEntryTemplateCommandHandler(repository, accountRepository, costCenterRepository, unitOfWork);

        var lines = new[]
        {
            new RecurringJournalEntryTemplateLineInput(Guid.NewGuid(), 500m, 0m),
            new RecurringJournalEntryTemplateLineInput(Guid.NewGuid(), 0m, 500m),
        };
        var command = new CreateRecurringJournalEntryTemplateCommand(Guid.NewGuid(), "Monthly rent", RecurringJournalEntryFrequency.Monthly, DateTimeOffset.UtcNow, lines);

        var result = await handler.Handle(command, CancellationToken.None);

        result.TotalDebit.Should().Be(500m);
        result.IsActive.Should().BeTrue();
        await repository.Received(1).AddAsync(Arg.Any<RecurringJournalEntryTemplate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAnAccountDoesNotExist_Throws()
    {
        var repository = Substitute.For<IRecurringJournalEntryTemplateRepository>();
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var costCenterRepository = Substitute.For<ICostCenterRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateRecurringJournalEntryTemplateCommandHandler(repository, accountRepository, costCenterRepository, unitOfWork);

        var lines = new[]
        {
            new RecurringJournalEntryTemplateLineInput(Guid.NewGuid(), 500m, 0m),
            new RecurringJournalEntryTemplateLineInput(Guid.NewGuid(), 0m, 500m),
        };
        var command = new CreateRecurringJournalEntryTemplateCommand(Guid.NewGuid(), "Monthly rent", RecurringJournalEntryFrequency.Monthly, DateTimeOffset.UtcNow, lines);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
