using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.BankStatementLines.Commands.ReconcileStatementLine;
using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;
using FusionOS.Modules.Finance.Domain.BankStatementLines;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.BankAccounts;

public class ReconcileStatementLineCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenLineExists_ReconcilesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var line = BankStatementLine.Create(companyId, Guid.NewGuid(), DateTimeOffset.UtcNow, 500m, "Customer wire");
        var repository = Substitute.For<IBankStatementLineRepository>();
        repository.GetByIdAsync(companyId, line.Id, Arg.Any<CancellationToken>()).Returns(line);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ReconcileStatementLineCommandHandler(repository, unitOfWork);
        var journalEntryId = Guid.NewGuid();

        var result = await handler.Handle(new ReconcileStatementLineCommand(companyId, line.Id, journalEntryId), CancellationToken.None);

        result.IsReconciled.Should().BeTrue();
        result.MatchedJournalEntryId.Should().Be(journalEntryId);
        result.ReconciledAt.Should().NotBeNull();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLineDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var repository = Substitute.For<IBankStatementLineRepository>();
        repository.GetByIdAsync(companyId, lineId, Arg.Any<CancellationToken>()).Returns((BankStatementLine?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ReconcileStatementLineCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new ReconcileStatementLineCommand(companyId, lineId, null), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
