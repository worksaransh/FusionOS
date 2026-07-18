using FluentAssertions;
using FusionOS.Modules.Finance.Application.BankAccounts.Contracts;
using FusionOS.Modules.Finance.Application.BankAccounts.Queries.ListBankAccounts;
using FusionOS.Modules.Finance.Domain.BankAccounts;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.BankAccounts;

public class ListBankAccountsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedBankAccountsForTheCompany()
    {
        var companyId = Guid.NewGuid();
        var bankAccounts = new[] { BankAccount.Create(companyId, "OPS-CHECKING", "Operating Checking", Guid.NewGuid(), null, null) };
        var repository = Substitute.For<IBankAccountRepository>();
        repository.ListAsync(companyId, null, 1, 25, Arg.Any<CancellationToken>()).Returns(bankAccounts);
        repository.CountAsync(companyId, null, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListBankAccountsQueryHandler(repository);

        var result = await handler.Handle(new ListBankAccountsQuery(companyId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(a => a.Code == "OPS-CHECKING");
    }
}
