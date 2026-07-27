using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.BankAccounts.Contracts;
using FusionOS.Modules.Finance.Application.BankStatementLines.Commands.ImportBankStatementLines;
using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;
using FusionOS.Modules.Finance.Domain.BankStatementLines;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.BankStatementLines;

/// <summary>
/// Covers ImportBankStatementLinesCommand (bulk-import follow-up to
/// RecordStatementLineCommand) — a valid row imports, a duplicate row is
/// skipped and counted rather than re-imported, and a row that fails
/// BankStatementLine.Create's own guards (e.g. a zero amount) is reported as
/// a per-row error without aborting the rest of the batch. Mirrors
/// RecordStatementLineCommandHandlerTests' bank-account-existence check.
/// </summary>
public class ImportBankStatementLinesCommandHandlerTests
{
    private static (IBankStatementLineRepository Repository, IBankAccountRepository BankAccountRepository, IUnitOfWork UnitOfWork, ImportBankStatementLinesCommandHandler Handler) CreateHandler(bool bankAccountExists = true)
    {
        var repository = Substitute.For<IBankStatementLineRepository>();
        var bankAccountRepository = Substitute.For<IBankAccountRepository>();
        bankAccountRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(bankAccountExists);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ImportBankStatementLinesCommandHandler(repository, bankAccountRepository, unitOfWork);
        return (repository, bankAccountRepository, unitOfWork, handler);
    }

    [Fact]
    public async Task Handle_WhenBankAccountDoesNotExist_Throws()
    {
        var (_, _, _, handler) = CreateHandler(bankAccountExists: false);
        var command = new ImportBankStatementLinesCommand(Guid.NewGuid(), Guid.NewGuid(), new[]
        {
            new ImportBankStatementLineInput(1, DateTimeOffset.UtcNow, 100m, "Wire in"),
        });

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenRowIsValidAndNotADuplicate_ImportsIt()
    {
        var (repository, _, unitOfWork, handler) = CreateHandler();
        repository.ExistsDuplicateAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        var command = new ImportBankStatementLinesCommand(Guid.NewGuid(), Guid.NewGuid(), new[]
        {
            new ImportBankStatementLineInput(1, DateTimeOffset.UtcNow, 250m, "Customer wire"),
        });

        var result = await handler.Handle(command, CancellationToken.None);

        result.RowsParsed.Should().Be(1);
        result.RowsImported.Should().Be(1);
        result.RowsSkippedAsDuplicate.Should().Be(0);
        result.Errors.Should().BeEmpty();
        await repository.Received(1).AddAsync(Arg.Any<BankStatementLine>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRowAlreadyExists_SkipsItAsADuplicateWithoutThrowing()
    {
        var (repository, _, unitOfWork, handler) = CreateHandler();
        repository.ExistsDuplicateAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);
        var command = new ImportBankStatementLinesCommand(Guid.NewGuid(), Guid.NewGuid(), new[]
        {
            new ImportBankStatementLineInput(1, DateTimeOffset.UtcNow, 250m, "Customer wire"),
        });

        var result = await handler.Handle(command, CancellationToken.None);

        result.RowsParsed.Should().Be(1);
        result.RowsImported.Should().Be(0);
        result.RowsSkippedAsDuplicate.Should().Be(1);
        result.Errors.Should().BeEmpty();
        await repository.DidNotReceive().AddAsync(Arg.Any<BankStatementLine>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOneRowIsMalformedAndAnotherIsValid_ReportsTheBadRowButStillImportsTheGoodOne()
    {
        var (repository, _, unitOfWork, handler) = CreateHandler();
        repository.ExistsDuplicateAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        var command = new ImportBankStatementLinesCommand(Guid.NewGuid(), Guid.NewGuid(), new[]
        {
            new ImportBankStatementLineInput(1, DateTimeOffset.UtcNow, 0m, "Zero amount row"),
            new ImportBankStatementLineInput(2, DateTimeOffset.UtcNow, 100m, "Good row"),
        });

        var result = await handler.Handle(command, CancellationToken.None);

        result.RowsParsed.Should().Be(2);
        result.RowsImported.Should().Be(1);
        result.RowsSkippedAsDuplicate.Should().Be(0);
        result.Errors.Should().ContainSingle(e => e.RowNumber == 1);
        await repository.Received(1).AddAsync(Arg.Any<BankStatementLine>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
