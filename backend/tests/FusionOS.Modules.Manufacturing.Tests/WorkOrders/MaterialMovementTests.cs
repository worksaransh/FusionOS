using FluentAssertions;
using FusionOS.Modules.Manufacturing.Domain.WorkOrders;
using FusionOS.Modules.Manufacturing.Domain.WorkOrders.Events;
using Xunit;

namespace FusionOS.Modules.Manufacturing.Tests.WorkOrders;

public class MaterialMovementTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid Bom = Guid.NewGuid();
    private static readonly Guid Product = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();
    private static readonly Guid ComponentA = Guid.NewGuid();

    private static WorkOrder CreateReleased(decimal componentQuantityPerUnit = 2m, decimal quantityToProduce = 10m)
    {
        var order = WorkOrder.Create(Company, Bom, Product, Warehouse, quantityToProduce, new[]
        {
            new BomComponentSnapshot(ComponentA, componentQuantityPerUnit),
        });
        order.Release();
        return order;
    }

    [Fact]
    public void IssueMaterial_WithinRequiredQuantity_UpdatesComponentAndRaisesEvent()
    {
        var order = CreateReleased(); // QuantityRequired = 2 * 10 = 20

        order.IssueMaterial(ComponentA, 12m);

        order.Components.Single(c => c.ComponentProductId == ComponentA).QuantityIssued.Should().Be(12m);
        order.DomainEvents.Should().ContainSingle(e => e is MaterialIssuedToWorkOrder);
    }

    [Fact]
    public void IssueMaterial_MoreThanRequired_Throws()
    {
        var order = CreateReleased(); // QuantityRequired = 20

        var act = () => order.IssueMaterial(ComponentA, 21m);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void IssueMaterial_WhileDraft_Throws()
    {
        var order = WorkOrder.Create(Company, Bom, Product, Warehouse, 10m, new[] { new BomComponentSnapshot(ComponentA, 2m) });

        var act = () => order.IssueMaterial(ComponentA, 1m);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void IssueMaterial_UnknownComponent_Throws()
    {
        var order = CreateReleased();

        var act = () => order.IssueMaterial(Guid.NewGuid(), 1m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ReturnMaterial_AfterIssue_ReducesQuantityIssued_AndRaisesEvent()
    {
        var order = CreateReleased();
        order.IssueMaterial(ComponentA, 12m);

        order.ReturnMaterial(ComponentA, 5m);

        order.Components.Single(c => c.ComponentProductId == ComponentA).QuantityIssued.Should().Be(7m);
        order.DomainEvents.Should().ContainSingle(e => e is MaterialReturnedFromWorkOrder);
    }

    [Fact]
    public void ReturnMaterial_MoreThanIssued_Throws()
    {
        var order = CreateReleased();
        order.IssueMaterial(ComponentA, 5m);

        var act = () => order.ReturnMaterial(ComponentA, 6m);

        act.Should().Throw<InvalidOperationException>();
    }
}
