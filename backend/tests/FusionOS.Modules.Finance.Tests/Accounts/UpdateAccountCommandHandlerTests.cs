using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Commands.UpdateAccount;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Domain.Accounts;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Accounts;

public class UpdateAccountCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenAccountExists_UpdatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var account = Account.Create(companyId, "1000", "Cash", AccountType.Asset, null);
        var repository = Substitute.For<IAccountRepository>();
        repository.GetByIdAsync(companyId, account.Id, Arg.Any<CancellationToken>()).Returns(account);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateAccountCommandHandler(repository, unitOfWork);
        var command = new UpdateAccountCommand(companyId, account.Id, "Petty Cash", "Asset", null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Petty Cash");
        result.Code.Should().Be("1000");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenParentAccountDoesNotExist_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var account = Account.Create(companyId, "1000", "Cash", AccountType.Asset, null);
        var repository = Substitute.For<IAccountRepository>();
        repository.GetByIdAsync(companyId, account.Id, Arg.Any<CancellationToken>()).Returns(account);
        repository.ExistsAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateAccountCommandHandler(repository, unitOfWork);
        var command = new UpdateAccountCommand(companyId, account.Id, "Cash", "Asset", Guid.NewGuid());

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenAccountDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var repository = Substitute.For<IAccountRepository>();
        repository.GetByIdAsync(companyId, accountId, Arg.Any<CancellationToken>()).Returns((Account?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateAccountCommandHandler(repository, unitOfWork);
        var command = new UpdateAccountCommand(companyId, accountId, "Cash", "Asset", null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
