using FusionOS.Modules.Warehouse.Domain.CycleCounts;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.CycleCounts;

public class CycleCountTests
{
    [Fact]
    public void Start_WithValidData_IsPending()
    {
        var cycleCount = CycleCount.Start(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m, Guid.NewGuid());

        cycleCount.Status.Should().Be(CycleCountStatus.Pending);
        cycleCount.SystemQuantitySnapshot.Should().Be(100m);
        cycleCount.CountedQuantity.Should().BeNull();
        cycleCount.VarianceQuantity.Should().BeNull();
    }

    [Fact]
    public void Start_WithoutBinId_Throws()
    {
        var act = () => CycleCount.Start(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), 100m, Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordCount_WithVariance_ComputesVarianceAndCompletes()
    {
        var cycleCount = CycleCount.Start(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m, Guid.NewGuid());

        cycleCount.RecordCount(92m);

        cycleCount.Status.Should().Be(CycleCountStatus.Completed);
        cycleCount.CountedQuantity.Should().Be(92m);
        cycleCount.VarianceQuantity.Should().Be(-8m);
    }

    [Fact]
    public void RecordCount_WithNoVariance_CompletesWithZeroVariance()
    {
        var cycleCount = CycleCount.Start(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m, Guid.NewGuid());

        cycleCount.RecordCount(100m);

        cycleCount.Status.Should().Be(CycleCountStatus.Completed);
        cycleCount.VarianceQuantity.Should().Be(0m);
    }

    [Fact]
    public void RecordCount_Twice_Throws()
    {
        var cycleCount = CycleCount.Start(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m, Guid.NewGuid());
        cycleCount.RecordCount(92m);

        var act = () => cycleCount.RecordCount(95m);

        act.Should().Throw<InvalidOperationException>();
    }
}
