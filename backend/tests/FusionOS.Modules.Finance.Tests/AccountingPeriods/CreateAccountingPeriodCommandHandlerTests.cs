using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Commands.CreateAccountingPeriod;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;
using FusionOS.Modules.Finance.Domain.AccountingPeriods;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.AccountingPeriods;

public class CreateAccountingPeriodCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenNoOverlap_PersistsPeriod()
    {
        var repository = Substitute.For<IAccountingPeriodRepository>();
        repository.ListAllAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(new List<AccountingPeriod>());
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateAccountingPeriodCommandHandler(repository, unitOfWork);
        var command = new CreateAccountingPeriodCommand(Guid.NewGuid(), new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero));

        var result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(nameof(AccountingPeriodStatus.Open));
        await repository.Received(1).AddAsync(Arg.Any<AccountingPeriod>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOverlappingAnExistingPeriod_Throws()
    {
        var companyId = Guid.NewGuid();
        var existing = AccountingPeriod.Create(companyId, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero));
        var repository = Substitute.For<IAccountingPeriodRepository>();
        repository.ListAllAsync(companyId, Arg.Any<CancellationToken>()).Returns(new List<AccountingPeriod> { existing });
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateAccountingPeriodCommandHandler(repository, unitOfWork);
        var command = new CreateAccountingPeriodCommand(companyId, new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 2, 15, 0, 0, 0, TimeSpan.Zero));

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<AccountingPeriod>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAdjacentButNotOverlapping_PersistsPeriod()
    {
        var companyId = Guid.NewGuid();
        var existing = AccountingPeriod.Create(companyId, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero));
        var repository = Substitute.For<IAccountingPeriodRepository>();
        repository.ListAllAsync(companyId, Arg.Any<CancellationToken>()).Returns(new List<AccountingPeriod> { existing });
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateAccountingPeriodCommandHandler(repository, unitOfWork);
        // Starts the instant the existing period ends' next day (00:00) — no overlap.
        var command = new CreateAccountingPeriodCommand(companyId, new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 2, 28, 0, 0, 0, TimeSpan.Zero));

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        await repository.Received(1).AddAsync(Arg.Any<AccountingPeriod>(), Arg.Any<CancellationToken>());
    }
}
