using FluentAssertions;
using FusionOS.Modules.BusinessIntelligence.Domain.KpiDefinitions;
using FusionOS.Modules.BusinessIntelligence.Domain.KpiDefinitions.Events;
using Xunit;

namespace FusionOS.Modules.BusinessIntelligence.Tests.KpiDefinitions;

public class KpiDefinitionTests
{
    private static readonly Guid Company = Guid.NewGuid();

    [Fact]
    public void Create_WithValidFields_RaisesCreatedEvent()
    {
        var kpi = KpiDefinition.Create(Company, "otd", "On-Time Delivery", "%");

        kpi.Code.Should().Be("OTD");
        kpi.Name.Should().Be("On-Time Delivery");
        kpi.Unit.Should().Be("%");
        kpi.IsActive.Should().BeTrue();
        kpi.DomainEvents.Should().ContainSingle(e => e is KpiDefinitionCreated);
    }

    [Fact]
    public void Create_WithNoUnit_LeavesUnitNull()
    {
        var kpi = KpiDefinition.Create(Company, "NPS", "Net Promoter Score", null);

        kpi.Unit.Should().BeNull();
    }

    [Fact]
    public void Create_WithBlankName_Throws()
    {
        var act = () => KpiDefinition.Create(Company, "OTD", "  ", null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var kpi = KpiDefinition.Create(Company, "OTD", "On-Time Delivery", "%");

        kpi.Deactivate();

        kpi.IsActive.Should().BeFalse();
    }
}
