using FluentAssertions;
using FusionOS.Modules.Manufacturing.Domain.WorkOrders;
using FusionOS.Modules.Manufacturing.Domain.WorkOrders.Events;
using Xunit;

namespace FusionOS.Modules.Manufacturing.Tests.WorkOrders;

public class ScrapYieldTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid Bom = Guid.NewGuid();
    private static readonly Guid Product = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();
    private static readonly Guid ComponentA = Guid.NewGuid();

    private static WorkOrder CreateReleased(decimal quantityToProduce = 10m)
    {
        var order = WorkOrder.Create(Company, Bom, Product, Warehouse, quantityToProduce, new[]
        {
            new BomComponentSnapshot(ComponentA, 1m),
        });
        order.Release();
        return order;
    }

    [Fact]
    public void Complete_WithNoArguments_Defaults100PercentYield_NoScrap()
    {
        var order = CreateReleased(quantityToProduce: 10m);

        order.Complete();

        order.QuantityGoodProduced.Should().Be(10m);
        order.QuantityScrapped.Should().Be(0m);
        order.YieldPercentage.Should().Be(100m);
        var evt = order.DomainEvents.OfType<WorkOrderCompleted>().Single();
        evt.QuantityProduced.Should().Be(10m);
        evt.QuantityScrapped.Should().Be(0m);
        evt.YieldPercentage.Should().Be(100m);
    }

    [Fact]
    public void Complete_WithGoodAndScrapped_ComputesYieldPercentage()
    {
        var order = CreateReleased(quantityToProduce: 10m);

        order.Complete(quantityGoodProduced: 8m, quantityScrapped: 2m);

        order.QuantityGoodProduced.Should().Be(8m);
        order.QuantityScrapped.Should().Be(2m);
        order.YieldPercentage.Should().Be(80m);
        var evt = order.DomainEvents.OfType<WorkOrderCompleted>().Single();
        evt.QuantityProduced.Should().Be(8m); // only the GOOD quantity is posted to Inventory
        evt.QuantityScrapped.Should().Be(2m);
        evt.YieldPercentage.Should().Be(80m);
    }

    [Fact]
    public void Complete_WithNegativeGoodQuantity_Throws()
    {
        var order = CreateReleased();

        var act = () => order.Complete(quantityGoodProduced: -1m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Complete_WithNegativeScrapQuantity_Throws()
    {
        var order = CreateReleased();

        var act = () => order.Complete(quantityScrapped: -1m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Complete_WithBothZero_Throws()
    {
        var order = CreateReleased();

        var act = () => order.Complete(quantityGoodProduced: 0m, quantityScrapped: 0m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Complete_FullyScrapped_ZeroPercentYield()
    {
        var order = CreateReleased(quantityToProduce: 10m);

        order.Complete(quantityGoodProduced: 0m, quantityScrapped: 10m);

        order.YieldPercentage.Should().Be(0m);
    }
}
