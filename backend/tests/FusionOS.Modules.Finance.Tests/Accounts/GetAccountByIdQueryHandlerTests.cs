using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Accounts.Queries.GetAccountById;
using FusionOS.Modules.Finance.Domain.Accounts;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Accounts;

public class GetAccountByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenAccountExists_ReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var account = Account.Create(companyId, "1000", "Cash", AccountType.Asset, null);
        var repository = Substitute.For<IAccountRepository>();
        repository.GetByIdAsync(companyId, account.Id, Arg.Any<CancellationToken>()).Returns(account);
        var handler = new GetAccountByIdQueryHandler(repository);

        var result = await handler.Handle(new GetAccountByIdQuery(companyId, account.Id), CancellationToken.None);

        result.Code.Should().Be("1000");
    }

    [Fact]
    public async Task Handle_WhenAccountDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var repository = Substitute.For<IAccountRepository>();
        repository.GetByIdAsync(companyId, accountId, Arg.Any<CancellationToken>()).Returns((Account?)null);
        var handler = new GetAccountByIdQueryHandler(repository);

        var act = () => handler.Handle(new GetAccountByIdQuery(companyId, accountId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
