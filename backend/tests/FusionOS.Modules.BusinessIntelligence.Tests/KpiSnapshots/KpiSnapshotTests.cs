using FluentAssertions;
using FusionOS.Modules.BusinessIntelligence.Domain.KpiSnapshots;
using FusionOS.Modules.BusinessIntelligence.Domain.KpiSnapshots.Events;
using Xunit;

namespace FusionOS.Modules.BusinessIntelligence.Tests.KpiSnapshots;

public class KpiSnapshotTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid Kpi = Guid.NewGuid();

    [Fact]
    public void Create_WithValidFields_RaisesRecordedEvent()
    {
        var snapshot = KpiSnapshot.Create(Company, Kpi, 97.5m, "Week 28");

        snapshot.Value.Should().Be(97.5m);
        snapshot.Notes.Should().Be("Week 28");
        snapshot.DomainEvents.Should().ContainSingle(e => e is KpiSnapshotRecorded);
    }

    [Fact]
    public void Create_WithNoNotes_LeavesNotesNull()
    {
        var snapshot = KpiSnapshot.Create(Company, Kpi, 97.5m, null);

        snapshot.Notes.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyKpiDefinitionId_Throws()
    {
        var act = () => KpiSnapshot.Create(Company, Guid.Empty, 97.5m, null);

        act.Should().Throw<ArgumentException>();
    }
}
