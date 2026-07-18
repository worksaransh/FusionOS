using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.BankAccounts.Commands.CreateBankAccount;
using FusionOS.Modules.Finance.Application.BankAccounts.Contracts;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.BankAccounts;

public class CreateBankAccountCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenLinkedAccountExistsAndCodeIsNew_PersistsBankAccount()
    {
        var repository = Substitute.For<IBankAccountRepository>();
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateBankAccountCommandHandler(repository, accountRepository, unitOfWork);
        var command = new CreateBankAccountCommand(Guid.NewGuid(), "OPS-CHECKING", "Operating Checking", Guid.NewGuid(), "First National", "1234");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Code.Should().Be("OPS-CHECKING");
        await repository.Received(1).AddAsync(Arg.Any<FusionOS.Modules.Finance.Domain.BankAccounts.BankAccount>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLinkedAccountDoesNotExist_Throws()
    {
        var repository = Substitute.For<IBankAccountRepository>();
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateBankAccountCommandHandler(repository, accountRepository, unitOfWork);
        var command = new CreateBankAccountCommand(Guid.NewGuid(), "OPS-CHECKING", "Operating Checking", Guid.NewGuid(), null, null);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenCodeAlreadyExists_Throws()
    {
        var repository = Substitute.For<IBankAccountRepository>();
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateBankAccountCommandHandler(repository, accountRepository, unitOfWork);
        var command = new CreateBankAccountCommand(Guid.NewGuid(), "OPS-CHECKING", "Operating Checking", Guid.NewGuid(), null, null);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
