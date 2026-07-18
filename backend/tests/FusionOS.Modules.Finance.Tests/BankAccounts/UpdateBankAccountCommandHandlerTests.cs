using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.BankAccounts.Commands.UpdateBankAccount;
using FusionOS.Modules.Finance.Application.BankAccounts.Contracts;
using FusionOS.Modules.Finance.Domain.BankAccounts;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.BankAccounts;

public class UpdateBankAccountCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenBankAccountExists_UpdatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var bankAccount = BankAccount.Create(companyId, "OPS-CHECKING", "Operating Checking", Guid.NewGuid(), null, null);
        var repository = Substitute.For<IBankAccountRepository>();
        repository.GetByIdAsync(companyId, bankAccount.Id, Arg.Any<CancellationToken>()).Returns(bankAccount);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateBankAccountCommandHandler(repository, unitOfWork);
        var command = new UpdateBankAccountCommand(companyId, bankAccount.Id, "Operating Checking (West)", "Second National", "5678");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Operating Checking (West)");
        result.BankName.Should().Be("Second National");
        result.AccountNumberLast4.Should().Be("5678");
        result.Code.Should().Be("OPS-CHECKING");
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
        var handler = new UpdateBankAccountCommandHandler(repository, unitOfWork);
        var command = new UpdateBankAccountCommand(companyId, bankAccountId, "Operating Checking", null, null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
