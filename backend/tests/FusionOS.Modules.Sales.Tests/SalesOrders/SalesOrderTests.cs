using FusionOS.Modules.Sales.Domain.SalesOrders;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.SalesOrders;

public class SalesOrderTests
{
    private static readonly SalesOrderLineInput[] OneLine =
    {
        new(Guid.NewGuid(), 3m, 100m),
    };

    [Fact]
    public void Create_WithValidLines_ComputesTotalAmount()
    {
        var order = SalesOrder.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);

        order.Status.Should().Be(SalesOrderStatus.Draft);
        order.TotalAmount.Should().Be(300m);
        order.DomainEvents.Should().ContainSingle(e => e is Events.SalesOrderCreated);
    }

    [Fact]
    public void Create_WithNoLines_Throws()
    {
        var act = () => SalesOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Array.Empty<SalesOrderLineInput>());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Confirm_FromDraft_TransitionsToConfirmedAndRaisesEvent()
    {
        var order = SalesOrder.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);
        order.ClearDomainEvents();

        order.Confirm();

        order.Status.Should().Be(SalesOrderStatus.Confirmed);
        order.DomainEvents.Should().ContainSingle(e => e is Events.SalesOrderConfirmed);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_Throws()
    {
        var order = SalesOrder.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);
        order.Confirm();

        var act = () => order.Confirm();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void FlagLineBackordered_WithValidQuantity_SetsFlagAndQuantity()
    {
        var order = SalesOrder.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);
        var lineId = order.Lines[0].Id;

        order.FlagLineBackordered(lineId, 2m);

        order.Lines[0].IsBackordered.Should().BeTrue();
        order.Lines[0].BackorderedQuantity.Should().Be(2m);
    }

    [Fact]
    public void FlagLineBackordered_WithQuantityExceedingLineQuantity_Throws()
    {
        var order = SalesOrder.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);
        var lineId = order.Lines[0].Id;

        var act = () => order.FlagLineBackordered(lineId, 100m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FlagLineBackordered_WithUnknownLineId_Throws()
    {
        var order = SalesOrder.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);

        var act = () => order.FlagLineBackordered(Guid.NewGuid(), 1m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ClearLineBackorder_AfterFlagged_ResetsFlagAndQuantity()
    {
        var order = SalesOrder.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);
        var lineId = order.Lines[0].Id;
        order.FlagLineBackordered(lineId, 2m);

        order.ClearLineBackorder(lineId);

        order.Lines[0].IsBackordered.Should().BeFalse();
        order.Lines[0].BackorderedQuantity.Should().Be(0m);
    }
}
