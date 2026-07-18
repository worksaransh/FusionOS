using FluentAssertions;
using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;
using FusionOS.Modules.Finance.Application.BankStatementLines.Queries.GetReconciliationSummary;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.BankAccounts;

public class GetReconciliationSummaryQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsSummaryForTheBankAccount()
    {
        var companyId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        var repository = Substitute.For<IBankStatementLineRepository>();
        repository.GetReconciliationSummaryAsync(companyId, bankAccountId, Arg.Any<CancellationToken>())
            .Returns((TotalLines: 5, ReconciledCount: 3, UnreconciledCount: 2, UnreconciledTotalAmount: 250.50m));
        var handler = new GetReconciliationSummaryQueryHandler(repository);

        var result = await handler.Handle(new GetReconciliationSummaryQuery(companyId, bankAccountId), CancellationToken.None);

        result.BankAccountId.Should().Be(bankAccountId);
        result.TotalLines.Should().Be(5);
        result.ReconciledCount.Should().Be(3);
        result.UnreconciledCount.Should().Be(2);
        result.UnreconciledTotalAmount.Should().Be(250.50m);
    }
}
