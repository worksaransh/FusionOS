using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Crm.Application.Accounts.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Application.Opportunities.Commands.AssignOpportunityAccount;
using FusionOS.Modules.Crm.Application.Opportunities.Commands.CreateOpportunity;
using FusionOS.Modules.Crm.Application.Opportunities.Commands.WinOpportunity;
using FusionOS.Modules.Crm.Application.Opportunities.Contracts;
using FusionOS.Modules.Crm.Domain.Leads;
using FusionOS.Modules.Crm.Domain.Opportunities;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Crm.Tests.Opportunities;

public class OpportunityCommandHandlerTests
{
    [Fact]
    public async Task CreateOpportunity_FromQualifiedLead_SnapshotsLeadAndConvertsIt()
    {
        var companyId = Guid.NewGuid();
        var lead = Lead.Create(companyId, "Acme Corp", "sales@acme.com", null, "Web");
        lead.Qualify();

        var leadRepository = Substitute.For<ILeadRepository>();
        leadRepository.GetByIdAsync(companyId, lead.Id, Arg.Any<CancellationToken>()).Returns(lead);
        var opportunityRepository = Substitute.For<IOpportunityRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateOpportunityCommandHandler(opportunityRepository, leadRepository, unitOfWork);

        var result = await handler.Handle(new CreateOpportunityCommand(companyId, lead.Id, "Acme deal", 25000m), CancellationToken.None);

        result.CustomerName.Should().Be("Acme Corp");
        result.Stage.Should().Be("Open");
        lead.Status.Should().Be(LeadStatus.Converted);
        await opportunityRepository.Received(1).AddAsync(Arg.Any<Opportunity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateOpportunity_WhenLeadMissing_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var leadRepository = Substitute.For<ILeadRepository>();
        leadRepository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Lead?)null);
        var handler = new CreateOpportunityCommandHandler(Substitute.For<IOpportunityRepository>(), leadRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new CreateOpportunityCommand(companyId, Guid.NewGuid(), "Deal", 1m), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task WinOpportunity_MarksWon()
    {
        var companyId = Guid.NewGuid();
        var opp = Opportunity.Create(companyId, Guid.NewGuid(), "Deal", "Acme Corp", "sales@acme.com", 1000m);

        var repository = Substitute.For<IOpportunityRepository>();
        repository.GetByIdAsync(companyId, opp.Id, Arg.Any<CancellationToken>()).Returns(opp);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new WinOpportunityCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new WinOpportunityCommand(companyId, opp.Id, "ACME"), CancellationToken.None);

        result.Stage.Should().Be("Won");
        result.CustomerCode.Should().Be("ACME");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignOpportunityAccount_WithExistingAccount_Assigns()
    {
        var companyId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var opp = Opportunity.Create(companyId, Guid.NewGuid(), "Deal", "Acme Corp", null, 1000m);

        var opportunityRepository = Substitute.For<IOpportunityRepository>();
        opportunityRepository.GetByIdAsync(companyId, opp.Id, Arg.Any<CancellationToken>()).Returns(opp);
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ExistsAsync(companyId, accountId, Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AssignOpportunityAccountCommandHandler(opportunityRepository, accountRepository, unitOfWork);

        var result = await handler.Handle(new AssignOpportunityAccountCommand(companyId, opp.Id, accountId), CancellationToken.None);

        result.AccountId.Should().Be(accountId);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignOpportunityAccount_WhenAccountMissing_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var opp = Opportunity.Create(companyId, Guid.NewGuid(), "Deal", "Acme Corp", null, 1000m);

        var opportunityRepository = Substitute.For<IOpportunityRepository>();
        opportunityRepository.GetByIdAsync(companyId, opp.Id, Arg.Any<CancellationToken>()).Returns(opp);
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ExistsAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var handler = new AssignOpportunityAccountCommandHandler(opportunityRepository, accountRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new AssignOpportunityAccountCommand(companyId, opp.Id, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
