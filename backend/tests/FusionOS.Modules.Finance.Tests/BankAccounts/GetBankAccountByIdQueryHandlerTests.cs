using FluentAssertions;
using FusionOS.Modules.Finance.Application.BankAccounts.Contracts;
using FusionOS.Modules.Finance.Application.BankAccounts.Queries.GetBankAccountById;
using FusionOS.Modules.Finance.Domain.BankAccounts;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.BankAccounts;

public class GetBankAccountByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenBankAccountExists_ReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var bankAccount = BankAccount.Create(companyId, "OPS-CHECKING", "Operating Checking", Guid.NewGuid(), null, null);
        var repository = Substitute.For<IBankAccountRepository>();
        repository.GetByIdAsync(companyId, bankAccount.Id, Arg.Any<CancellationToken>()).Returns(bankAccount);
        var handler = new GetBankAccountByIdQueryHandler(repository);

        var result = await handler.Handle(new GetBankAccountByIdQuery(companyId, bankAccount.Id), CancellationToken.None);

        result.Code.Should().Be("OPS-CHECKING");
    }

    [Fact]
    public async Task Handle_WhenBankAccountDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        var repository = Substitute.For<IBankAccountRepository>();
        repository.GetByIdAsync(companyId, bankAccountId, Arg.Any<CancellationToken>()).Returns((BankAccount?)null);
        var handler = new GetBankAccountByIdQueryHandler(repository);

        var act = () => handler.Handle(new GetBankAccountByIdQuery(companyId, bankAccountId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
