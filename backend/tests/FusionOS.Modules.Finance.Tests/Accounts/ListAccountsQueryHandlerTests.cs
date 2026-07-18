using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Accounts.Queries.ListAccounts;
using FusionOS.Modules.Finance.Domain.Accounts;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Accounts;

public class ListAccountsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedAccountsForTheCompany()
    {
        var companyId = Guid.NewGuid();
        var accounts = new[] { Account.Create(companyId, "1000", "Cash", AccountType.Asset, null) };
        var repository = Substitute.For<IAccountRepository>();
        repository.ListAsync(companyId, null, 1, 25, Arg.Any<CancellationToken>()).Returns(accounts);
        repository.CountAsync(companyId, null, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListAccountsQueryHandler(repository);

        var result = await handler.Handle(new ListAccountsQuery(companyId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(a => a.Code == "1000");
    }
}
