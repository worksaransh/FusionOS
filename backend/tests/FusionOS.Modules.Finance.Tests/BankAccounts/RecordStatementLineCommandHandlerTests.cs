using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.BankAccounts.Contracts;
using FusionOS.Modules.Finance.Application.BankStatementLines.Commands.RecordStatementLine;
using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.BankAccounts;

public class RecordStatementLineCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenBankAccountExists_PersistsLine()
    {
        var repository = Substitute.For<IBankStatementLineRepository>();
        var bankAccountRepository = Substitute.For<IBankAccountRepository>();
        bankAccountRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordStatementLineCommandHandler(repository, bankAccountRepository, unitOfWork);
        var command = new RecordStatementLineCommand(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, 500m, "Customer wire");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Amount.Should().Be(500m);
        result.IsReconciled.Should().BeFalse();
        await repository.Received(1).AddAsync(Arg.Any<FusionOS.Modules.Finance.Domain.BankStatementLines.BankStatementLine>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenBankAccountDoesNotExist_Throws()
    {
        var repository = Substitute.For<IBankStatementLineRepository>();
        var bankAccountRepository = Substitute.For<IBankAccountRepository>();
        bankAccountRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordStatementLineCommandHandler(repository, bankAccountRepository, unitOfWork);
        var command = new RecordStatementLineCommand(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, 500m, "Customer wire");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
