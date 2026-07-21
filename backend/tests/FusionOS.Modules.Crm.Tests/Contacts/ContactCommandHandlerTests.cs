using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Crm.Application.Accounts.Contracts;
using FusionOS.Modules.Crm.Application.Contacts.Commands.CreateContact;
using FusionOS.Modules.Crm.Application.Contacts.Commands.DeactivateContact;
using FusionOS.Modules.Crm.Application.Contacts.Commands.UpdateContact;
using FusionOS.Modules.Crm.Application.Contacts.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Domain.Contacts;
using FusionOS.Modules.Crm.Domain.Leads;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Crm.Tests.Contacts;

public class ContactCommandHandlerTests
{
    [Fact]
    public async Task CreateContact_WithoutAccountOrLead_Persists()
    {
        var companyId = Guid.NewGuid();
        var contactRepository = Substitute.For<IContactRepository>();
        var accountRepository = Substitute.For<IAccountRepository>();
        var leadRepository = Substitute.For<ILeadRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateContactCommandHandler(contactRepository, accountRepository, leadRepository, unitOfWork);

        var result = await handler.Handle(new CreateContactCommand(companyId, "Jane Doe", "jane@acme.com", null, null, null, null), CancellationToken.None);

        result.Name.Should().Be("Jane Doe");
        await contactRepository.Received(1).AddAsync(Arg.Any<Contact>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateContact_WhenAccountMissing_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ExistsAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var handler = new CreateContactCommandHandler(
            Substitute.For<IContactRepository>(), accountRepository, Substitute.For<ILeadRepository>(), Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new CreateContactCommand(companyId, "Jane Doe", null, null, null, Guid.NewGuid(), null), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateContact_WhenLeadMissing_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var leadRepository = Substitute.For<ILeadRepository>();
        leadRepository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Lead?)null);
        var handler = new CreateContactCommandHandler(
            Substitute.For<IContactRepository>(), Substitute.For<IAccountRepository>(), leadRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new CreateContactCommand(companyId, "Jane Doe", null, null, null, null, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateContact_RePointsFromLeadToAccount()
    {
        var companyId = Guid.NewGuid();
        var leadId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var contact = Contact.Create(companyId, "Jane Doe", null, null, null, null, leadId);

        var contactRepository = Substitute.For<IContactRepository>();
        contactRepository.GetByIdAsync(companyId, contact.Id, Arg.Any<CancellationToken>()).Returns(contact);
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ExistsAsync(companyId, accountId, Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateContactCommandHandler(contactRepository, accountRepository, Substitute.For<ILeadRepository>(), unitOfWork);

        var result = await handler.Handle(
            new UpdateContactCommand(companyId, contact.Id, "Jane Doe", null, null, null, accountId, null), CancellationToken.None);

        result.AccountId.Should().Be(accountId);
        result.LeadId.Should().BeNull();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateContact_SetsIsActiveFalse()
    {
        var companyId = Guid.NewGuid();
        var contact = Contact.Create(companyId, "Jane Doe", null, null, null, null, null);

        var repository = Substitute.For<IContactRepository>();
        repository.GetByIdAsync(companyId, contact.Id, Arg.Any<CancellationToken>()).Returns(contact);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateContactCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateContactCommand(companyId, contact.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
    }
}
