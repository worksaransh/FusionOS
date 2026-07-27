using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Commands.ReopenAccountingPeriod;
using FusionOS.Modules.Finance.Domain.AccountingPeriods;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.AccountingPeriods;

public class ReopenAccountingPeriodCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenClosed_ReopensPeriod()
    {
        var companyId = Guid.NewGuid();
        var period = AccountingPeriod.Create(companyId, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(30));
        period.Close();
        var repository = Substitute.For<IAccountingPeriodRepository>();
        repository.GetByIdAsync(companyId, period.Id, Arg.Any<CancellationToken>()).Returns(period);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ReopenAccountingPeriodCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new ReopenAccountingPeriodCommand(companyId, period.Id), CancellationToken.None);

        result.Status.Should().Be(nameof(AccountingPeriodStatus.Open));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAlreadyOpen_Throws()
    {
        var companyId = Guid.NewGuid();
        var period = AccountingPeriod.Create(companyId, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(30));
        var repository = Substitute.For<IAccountingPeriodRepository>();
        repository.GetByIdAsync(companyId, period.Id, Arg.Any<CancellationToken>()).Returns(period);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ReopenAccountingPeriodCommandHandler(repository, unitOfWork);

        var act = async () => await handler.Handle(new ReopenAccountingPeriodCommand(companyId, period.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
