using FluentAssertions;
using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;
using FusionOS.Modules.Finance.Application.BankStatementLines.Queries.ListBankStatementLines;
using FusionOS.Modules.Finance.Domain.BankStatementLines;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.BankAccounts;

public class ListBankStatementLinesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedLinesForTheBankAccount()
    {
        var companyId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        var line = BankStatementLine.Create(companyId, bankAccountId, DateTimeOffset.UtcNow, 500m, "Customer wire");
        var repository = Substitute.For<IBankStatementLineRepository>();
        repository.ListByBankAccountAsync(companyId, bankAccountId, null, 1, 25, Arg.Any<CancellationToken>()).Returns(new[] { line });
        repository.CountByBankAccountAsync(companyId, bankAccountId, null, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListBankStatementLinesQueryHandler(repository);

        var result = await handler.Handle(new ListBankStatementLinesQuery(companyId, bankAccountId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(l => l.Amount == 500m);
    }
}
