using FluentAssertions;
using FusionOS.Modules.Crm.Application.Accounts.Commands.CreateAccount;
using FusionOS.Modules.Crm.Application.Accounts.Commands.DeactivateAccount;
using FusionOS.Modules.Crm.Application.Accounts.Commands.UpdateAccount;
using FusionOS.Modules.Crm.Application.Accounts.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Domain.Accounts;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Crm.Tests.Accounts;

public class AccountCommandHandlerTests
{
    [Fact]
    public async Task CreateAccount_Persists()
    {
        var repository = Substitute.For<IAccountRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateAccountCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new CreateAccountCommand(Guid.NewGuid(), "Acme Corp", "Manufacturing", null), CancellationToken.None);

        result.Name.Should().Be("Acme Corp");
        await repository.Received(1).AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAccount_ChangesFields()
    {
        var companyId = Guid.NewGuid();
        var account = Account.Create(companyId, "Acme Corp", "Manufacturing", null);

        var repository = Substitute.For<IAccountRepository>();
        repository.GetByIdAsync(companyId, account.Id, Arg.Any<CancellationToken>()).Returns(account);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateAccountCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new UpdateAccountCommand(companyId, account.Id, "Acme Holdings", "Logistics", "https://acme.example"), CancellationToken.None);

        result.Name.Should().Be("Acme Holdings");
        result.Website.Should().Be("https://acme.example");
    }

    [Fact]
    public async Task DeactivateAccount_SetsIsActiveFalse()
    {
        var companyId = Guid.NewGuid();
        var account = Account.Create(companyId, "Acme Corp", null, null);

        var repository = Substitute.For<IAccountRepository>();
        repository.GetByIdAsync(companyId, account.Id, Arg.Any<CancellationToken>()).Returns(account);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateAccountCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateAccountCommand(companyId, account.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
    }
}
