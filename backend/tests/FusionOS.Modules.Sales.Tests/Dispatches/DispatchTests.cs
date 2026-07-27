using FusionOS.Modules.Sales.Domain.Dispatches;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Dispatches;

/// <summary>Covers Dispatch.Create's optional PickListId parameter (2026-07-21 Dispatch-PickList link follow-up).</summary>
public class DispatchTests
{
    [Fact]
    public void Create_WithoutPickListId_DefaultsToNull()
    {
        var dispatch = Dispatch.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new[] { new DispatchLineInput(Guid.NewGuid(), 5m) });

        dispatch.PickListId.Should().BeNull();
    }

    [Fact]
    public void Create_WithPickListId_SetsIt()
    {
        var pickListId = Guid.NewGuid();

        var dispatch = Dispatch.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new[] { new DispatchLineInput(Guid.NewGuid(), 5m) }, pickListId);

        dispatch.PickListId.Should().Be(pickListId);
    }

    [Fact]
    public void Create_WithEmptyPickListId_Throws()
    {
        var act = () => Dispatch.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new[] { new DispatchLineInput(Guid.NewGuid(), 5m) }, Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }
}
