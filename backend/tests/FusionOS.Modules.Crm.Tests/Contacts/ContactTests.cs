using FluentAssertions;
using FusionOS.Modules.Crm.Domain.Contacts;
using FusionOS.Modules.Crm.Domain.Contacts.Events;
using Xunit;

namespace FusionOS.Modules.Crm.Tests.Contacts;

public class ContactTests
{
    private static readonly Guid Company = Guid.NewGuid();

    private static Contact New(Guid? accountId = null, Guid? leadId = null) =>
        Contact.Create(Company, " Jane Doe ", "Jane@Acme.COM", "555-1234", "VP Sales", accountId, leadId);

    [Fact]
    public void Create_NormalizesFields_AndRaisesEvent()
    {
        var contact = New();

        contact.Name.Should().Be("Jane Doe");
        contact.Email.Should().Be("jane@acme.com");
        contact.IsActive.Should().BeTrue();
        contact.DomainEvents.Should().ContainSingle(e => e is ContactCreated);
    }

    [Fact]
    public void Create_WithInvalidEmail_Throws()
    {
        var act = () => Contact.Create(Company, "Jane Doe", "not-an-email", null, null, null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_CanReferenceLeadWithoutAccount()
    {
        var leadId = Guid.NewGuid();

        var contact = Contact.Create(Company, "Jane Doe", null, null, null, null, leadId);

        contact.AccountId.Should().BeNull();
        contact.LeadId.Should().Be(leadId);
    }

    [Fact]
    public void UpdateDetails_CanRePointFromLeadToAccount()
    {
        var leadId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var contact = New(leadId: leadId);

        contact.UpdateDetails("Jane Doe", "jane@acme.com", "555-1234", "VP Sales", accountId, null);

        contact.AccountId.Should().Be(accountId);
        contact.LeadId.Should().BeNull();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var contact = New();

        contact.Deactivate();

        contact.IsActive.Should().BeFalse();
    }
}
