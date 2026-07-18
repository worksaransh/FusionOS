using FusionOS.Modules.Warehouse.Domain.PickLists;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.PickLists;

public class PickListTests
{
    private static PickListLineInput Line(decimal quantityToPick = 10m, Guid? binId = null) =>
        new(Guid.NewGuid(), binId, quantityToPick);

    [Fact]
    public void Create_WithValidData_IsPending()
    {
        var pickList = PickList.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new[] { Line() });

        pickList.Status.Should().Be(PickListStatus.Pending);
        pickList.AssignedToUserId.Should().BeNull();
        pickList.Lines.Should().HaveCount(1);
    }

    [Fact]
    public void Create_WithNoLines_Throws()
    {
        var act = () => PickList.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Array.Empty<PickListLineInput>());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AssignTo_FromPending_AdvancesToAssigned()
    {
        var pickList = PickList.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new[] { Line() });
        var userId = Guid.NewGuid();

        pickList.AssignTo(userId);

        pickList.Status.Should().Be(PickListStatus.Assigned);
        pickList.AssignedToUserId.Should().Be(userId);
    }

    [Fact]
    public void AssignTo_WhenPacked_Throws()
    {
        var pickList = PickList.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new[] { Line(5m) });
        pickList.AssignTo(Guid.NewGuid());
        pickList.RecordPick(pickList.Lines[0].Id, 5m);
        pickList.Pack();

        var act = () => pickList.AssignTo(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RecordPick_WithoutAssignment_Throws()
    {
        var pickList = PickList.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new[] { Line() });

        var act = () => pickList.RecordPick(pickList.Lines[0].Id, 5m);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RecordPick_PartialQuantity_KeepsStatusAssigned()
    {
        var pickList = PickList.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new[] { Line(10m) });
        pickList.AssignTo(Guid.NewGuid());

        pickList.RecordPick(pickList.Lines[0].Id, 4m);

        pickList.Status.Should().Be(PickListStatus.Assigned);
        pickList.Lines[0].QuantityPicked.Should().Be(4m);
        pickList.Lines[0].IsFullyPicked.Should().BeFalse();
    }

    [Fact]
    public void RecordPick_AllLinesFullyPicked_AdvancesToPicked()
    {
        var pickList = PickList.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new[] { Line(10m), Line(5m) });
        pickList.AssignTo(Guid.NewGuid());

        pickList.RecordPick(pickList.Lines[0].Id, 10m);
        pickList.Status.Should().Be(PickListStatus.Assigned);

        pickList.RecordPick(pickList.Lines[1].Id, 5m);
        pickList.Status.Should().Be(PickListStatus.Picked);
    }

    [Fact]
    public void RecordPick_ExceedingQuantityToPick_Throws()
    {
        var pickList = PickList.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new[] { Line(10m) });
        pickList.AssignTo(Guid.NewGuid());

        var act = () => pickList.RecordPick(pickList.Lines[0].Id, 11m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Pack_WhenNotFullyPicked_Throws()
    {
        var pickList = PickList.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new[] { Line(10m) });
        pickList.AssignTo(Guid.NewGuid());
        pickList.RecordPick(pickList.Lines[0].Id, 4m);

        var act = () => pickList.Pack();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Pack_WhenFullyPicked_AdvancesToPacked()
    {
        var pickList = PickList.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new[] { Line(10m) });
        pickList.AssignTo(Guid.NewGuid());
        pickList.RecordPick(pickList.Lines[0].Id, 10m);

        pickList.Pack();

        pickList.Status.Should().Be(PickListStatus.Packed);
    }
}
