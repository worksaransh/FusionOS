using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.BankStatementLines.Commands.UnreconcileStatementLine;
using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;
using FusionOS.Modules.Finance.Domain.BankStatementLines;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.BankAccounts;

public class UnreconcileStatementLineCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenLineExists_UnreconcilesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var line = BankStatementLine.Create(companyId, Guid.NewGuid(), DateTimeOffset.UtcNow, 500m, "Customer wire");
        line.Reconcile(Guid.NewGuid());
        var repository = Substitute.For<IBankStatementLineRepository>();
        repository.GetByIdAsync(companyId, line.Id, Arg.Any<CancellationToken>()).Returns(line);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UnreconcileStatementLineCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new UnreconcileStatementLineCommand(companyId, line.Id), CancellationToken.None);

        result.IsReconciled.Should().BeFalse();
        result.MatchedJournalEntryId.Should().BeNull();
        result.ReconciledAt.Should().BeNull();
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
        var handler = new UnreconcileStatementLineCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new UnreconcileStatementLineCommand(companyId, lineId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
