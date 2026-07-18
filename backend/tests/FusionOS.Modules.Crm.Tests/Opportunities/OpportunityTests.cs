using FluentAssertions;
using FusionOS.Modules.Crm.Domain.Opportunities;
using FusionOS.Modules.Crm.Domain.Opportunities.Events;
using Xunit;

namespace FusionOS.Modules.Crm.Tests.Opportunities;

public class OpportunityTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid Lead = Guid.NewGuid();

    private static Opportunity Open() =>
        Opportunity.Create(Company, Lead, "Acme - annual license", "Acme Corp", "sales@acme.com", 50000m);

    [Fact]
    public void Create_Open_RaisesOpportunityCreated()
    {
        var opp = Open();

        opp.Stage.Should().Be(OpportunityStage.Open);
        opp.CustomerName.Should().Be("Acme Corp");
        opp.DomainEvents.Should().ContainSingle(e => e is OpportunityCreated);
    }

    [Fact]
    public void Win_NormalizesCode_AndRaisesOpportunityWon()
    {
        var opp = Open();

        opp.Win(" acme ");

        opp.Stage.Should().Be(OpportunityStage.Won);
        opp.CustomerCode.Should().Be("ACME");
        var evt = opp.DomainEvents.OfType<OpportunityWon>().Single();
        evt.CustomerName.Should().Be("Acme Corp");
        evt.CustomerCode.Should().Be("ACME");
        evt.ContactEmail.Should().Be("sales@acme.com");
    }

    [Fact]
    public void Win_RequiresCustomerCode()
    {
        var opp = Open();

        var act = () => opp.Win("  ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Win_WhenNotOpen_Throws()
    {
        var opp = Open();
        opp.Win("ACME");

        var act = () => opp.Win("ACME2");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Lose_FromOpen_SetsLost()
    {
        var opp = Open();

        opp.Lose();

        opp.Stage.Should().Be(OpportunityStage.Lost);
    }
}
