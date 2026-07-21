using FluentAssertions;
using FusionOS.Modules.Crm.Domain.Leads;
using FusionOS.Modules.Crm.Domain.Leads.Events;
using Xunit;

namespace FusionOS.Modules.Crm.Tests.Leads;

public class LeadTests
{
    private static readonly Guid Company = Guid.NewGuid();

    private static Lead New() => Lead.Create(Company, " Acme Corp ", "Sales@Acme.COM", "555-1234", "Web");

    [Fact]
    public void Create_NormalizesFields_AndRaisesEvent()
    {
        var lead = New();

        lead.Name.Should().Be("Acme Corp");
        lead.ContactEmail.Should().Be("sales@acme.com");
        lead.Status.Should().Be(LeadStatus.New);
        lead.DomainEvents.Should().ContainSingle(e => e is LeadCreated);
    }

    [Fact]
    public void Qualify_FromNew_SetsQualified()
    {
        var lead = New();
        lead.Qualify();
        lead.Status.Should().Be(LeadStatus.Qualified);
    }

    [Fact]
    public void MarkConverted_RequiresQualified()
    {
        var lead = New();

        var act = () => lead.MarkConverted();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkConverted_FromQualified_Succeeds()
    {
        var lead = New();
        lead.Qualify();

        lead.MarkConverted();

        lead.Status.Should().Be(LeadStatus.Converted);
    }

    [Fact]
    public void Disqualify_AfterConversion_Throws()
    {
        var lead = New();
        lead.Qualify();
        lead.MarkConverted();

        var act = () => lead.Disqualify();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AssignAccount_SetsAndClearsAccountId()
    {
        var lead = New();
        var accountId = Guid.NewGuid();

        lead.AssignAccount(accountId);
        lead.AccountId.Should().Be(accountId);

        lead.AssignAccount(null);
        lead.AccountId.Should().BeNull();
    }
}
