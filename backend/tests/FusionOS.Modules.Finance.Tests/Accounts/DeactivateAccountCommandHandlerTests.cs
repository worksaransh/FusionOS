using FusionOS.Modules.Finance.Application.Accounts.Commands.DeactivateAccount;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Domain.Accounts;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Accounts;

public class DeactivateAccountCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenAccountExists_DeactivatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var account = Account.Create(companyId, "1000", "Cash", AccountType.Asset, null);
        var repository = Substitute.For<IAccountRepository>();
        repository.GetByIdAsync(companyId, account.Id, Arg.Any<CancellationToken>()).Returns(account);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateAccountCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateAccountCommand(companyId, account.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAccountDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var repository = Substitute.For<IAccountRepository>();
        repository.GetByIdAsync(companyId, accountId, Arg.Any<CancellationToken>()).Returns((Account?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateAccountCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new DeactivateAccountCommand(companyId, accountId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
