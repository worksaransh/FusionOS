using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.BankAccounts.Commands.DeactivateBankAccount;
using FusionOS.Modules.Finance.Application.BankAccounts.Contracts;
using FusionOS.Modules.Finance.Domain.BankAccounts;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.BankAccounts;

public class DeactivateBankAccountCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenBankAccountExists_DeactivatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var bankAccount = BankAccount.Create(companyId, "OPS-CHECKING", "Operating Checking", Guid.NewGuid(), null, null);
        var repository = Substitute.For<IBankAccountRepository>();
        repository.GetByIdAsync(companyId, bankAccount.Id, Arg.Any<CancellationToken>()).Returns(bankAccount);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateBankAccountCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateBankAccountCommand(companyId, bankAccount.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenBankAccountDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        var repository = Substitute.For<IBankAccountRepository>();
        repository.GetByIdAsync(companyId, bankAccountId, Arg.Any<CancellationToken>()).Returns((BankAccount?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateBankAccountCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new DeactivateBankAccountCommand(companyId, bankAccountId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
