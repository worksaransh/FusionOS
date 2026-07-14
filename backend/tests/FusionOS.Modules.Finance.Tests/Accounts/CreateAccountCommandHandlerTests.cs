using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Commands.CreateAccount;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Accounts;

public class CreateAccountCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCodeIsNew_PersistsAccount()
    {
        var repository = Substitute.For<IAccountRepository>();
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateAccountCommandHandler(repository, unitOfWork);
        var command = new CreateAccountCommand(Guid.NewGuid(), "1000", "Cash", "Asset", null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Code.Should().Be("1000");
        await repository.Received(1).AddAsync(Arg.Any<FusionOS.Modules.Finance.Domain.Accounts.Account>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCodeAlreadyExists_Throws()
    {
        var repository = Substitute.For<IAccountRepository>();
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateAccountCommandHandler(repository, unitOfWork);
        var command = new CreateAccountCommand(Guid.NewGuid(), "1000", "Cash", "Asset", null);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenParentAccountDoesNotExist_Throws()
    {
        var repository = Substitute.For<IAccountRepository>();
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        repository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateAccountCommandHandler(repository, unitOfWork);
        var command = new CreateAccountCommand(Guid.NewGuid(), "1100", "Current Assets", "Asset", Guid.NewGuid());

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
