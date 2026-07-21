using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Settings.Commands.UpdateFinanceSettings;
using FusionOS.Modules.Finance.Application.Settings.Contracts;
using FusionOS.Modules.Finance.Domain.Settings;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Settings;

public class UpdateFinanceSettingsCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenSettingsDoNotExistYet_CreatesThenConfiguresInOneStep()
    {
        var companyId = Guid.NewGuid();
        var arAccountId = Guid.NewGuid();
        var revenueAccountId = Guid.NewGuid();
        var repository = Substitute.For<IFinanceSettingsRepository>();
        repository.GetByCompanyIdAsync(companyId, Arg.Any<CancellationToken>()).Returns((FinanceSettings?)null);
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ExistsAsync(companyId, arAccountId, Arg.Any<CancellationToken>()).Returns(true);
        accountRepository.ExistsAsync(companyId, revenueAccountId, Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateFinanceSettingsCommandHandler(repository, accountRepository, unitOfWork);
        var command = new UpdateFinanceSettingsCommand(companyId, arAccountId, revenueAccountId, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.DefaultArAccountId.Should().Be(arAccountId);
        result.DefaultSalesRevenueAccountId.Should().Be(revenueAccountId);
        await repository.Received(1).AddAsync(Arg.Any<FinanceSettings>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAnAccountIdDoesNotExist_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var bogusAccountId = Guid.NewGuid();
        var repository = Substitute.For<IFinanceSettingsRepository>();
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ExistsAsync(companyId, bogusAccountId, Arg.Any<CancellationToken>()).Returns(false);
        var handler = new UpdateFinanceSettingsCommandHandler(repository, accountRepository, Substitute.For<IUnitOfWork>());
        var command = new UpdateFinanceSettingsCommand(companyId, bogusAccountId, null, null, null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WithAllNullAccountIds_SkipsExistenceChecksEntirely()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IFinanceSettingsRepository>();
        repository.GetByCompanyIdAsync(companyId, Arg.Any<CancellationToken>()).Returns((FinanceSettings?)null);
        var accountRepository = Substitute.For<IAccountRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateFinanceSettingsCommandHandler(repository, accountRepository, unitOfWork);
        var command = new UpdateFinanceSettingsCommand(companyId, null, null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.DefaultArAccountId.Should().BeNull();
        await accountRepository.DidNotReceive().ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
