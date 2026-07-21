using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Crm.Application.Accounts.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Commands.AssignLeadAccount;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Domain.Leads;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Crm.Tests.Leads;

public class LeadCommandHandlerTests
{
    [Fact]
    public async Task AssignLeadAccount_WithExistingAccount_Assigns()
    {
        var companyId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var lead = Lead.Create(companyId, "Acme Corp", null, null, null);

        var leadRepository = Substitute.For<ILeadRepository>();
        leadRepository.GetByIdAsync(companyId, lead.Id, Arg.Any<CancellationToken>()).Returns(lead);
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ExistsAsync(companyId, accountId, Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AssignLeadAccountCommandHandler(leadRepository, accountRepository, unitOfWork);

        var result = await handler.Handle(new AssignLeadAccountCommand(companyId, lead.Id, accountId), CancellationToken.None);

        result.AccountId.Should().Be(accountId);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignLeadAccount_WhenAccountMissing_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var lead = Lead.Create(companyId, "Acme Corp", null, null, null);

        var leadRepository = Substitute.For<ILeadRepository>();
        leadRepository.GetByIdAsync(companyId, lead.Id, Arg.Any<CancellationToken>()).Returns(lead);
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ExistsAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var handler = new AssignLeadAccountCommandHandler(leadRepository, accountRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new AssignLeadAccountCommand(companyId, lead.Id, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
