using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Commands.CloseAccountingPeriod;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Domain.AccountingPeriods;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.AccountingPeriods;

public class CloseAccountingPeriodCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOpenAndNoOutstandingDrafts_ClosesPeriod()
    {
        var companyId = Guid.NewGuid();
        var period = AccountingPeriod.Create(companyId, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(30));
        var repository = Substitute.For<IAccountingPeriodRepository>();
        repository.GetByIdAsync(companyId, period.Id, Arg.Any<CancellationToken>()).Returns(period);
        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        journalEntryRepository.ExistsDraftEntryInDateRangeAsync(companyId, period.PeriodStart, period.PeriodEnd, Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CloseAccountingPeriodCommandHandler(repository, journalEntryRepository, unitOfWork);

        var result = await handler.Handle(new CloseAccountingPeriodCommand(companyId, period.Id), CancellationToken.None);

        result.Status.Should().Be(nameof(AccountingPeriodStatus.Closed));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOutstandingDraftsExistInRange_Throws()
    {
        var companyId = Guid.NewGuid();
        var period = AccountingPeriod.Create(companyId, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(30));
        var repository = Substitute.For<IAccountingPeriodRepository>();
        repository.GetByIdAsync(companyId, period.Id, Arg.Any<CancellationToken>()).Returns(period);
        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        journalEntryRepository.ExistsDraftEntryInDateRangeAsync(companyId, period.PeriodStart, period.PeriodEnd, Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CloseAccountingPeriodCommandHandler(repository, journalEntryRepository, unitOfWork);

        var act = () => handler.Handle(new CloseAccountingPeriodCommand(companyId, period.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        period.Status.Should().Be(AccountingPeriodStatus.Open);
    }

    [Fact]
    public async Task Handle_WhenPeriodNotFound_Throws()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IAccountingPeriodRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AccountingPeriod?)null);
        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CloseAccountingPeriodCommandHandler(repository, journalEntryRepository, unitOfWork);

        var act = async () => await handler.Handle(new CloseAccountingPeriodCommand(companyId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
