using FluentAssertions;
using FusionOS.Modules.Manufacturing.Domain.WorkOrders;
using FusionOS.Modules.Manufacturing.Domain.WorkOrders.Events;
using Xunit;

namespace FusionOS.Modules.Manufacturing.Tests.WorkOrders;

public class WorkOrderTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid Bom = Guid.NewGuid();
    private static readonly Guid Product = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();
    private static readonly Guid ComponentA = Guid.NewGuid();
    private static readonly Guid ComponentB = Guid.NewGuid();

    private static WorkOrder Create(decimal quantity = 10m) =>
        WorkOrder.Create(Company, Bom, Product, Warehouse, quantity, new[]
        {
            new BomComponentSnapshot(ComponentA, 2m),
            new BomComponentSnapshot(ComponentB, 0.5m),
        });

    [Fact]
    public void Create_SnapshotsComponentsScaledByQuantity()
    {
        var order = Create(quantity: 10m);

        order.Status.Should().Be(WorkOrderStatus.Draft);
        order.Components.Should().HaveCount(2);
        order.Components.Single(c => c.ComponentProductId == ComponentA).QuantityRequired.Should().Be(20m); // 2 * 10
        order.Components.Single(c => c.ComponentProductId == ComponentB).QuantityRequired.Should().Be(5m); // 0.5 * 10
    }

    [Fact]
    public void Complete_FromReleased_RaisesWorkOrderCompletedWithConsumptions()
    {
        var order = Create(quantity: 10m);
        order.Release();

        order.Complete();

        order.Status.Should().Be(WorkOrderStatus.Completed);
        var evt = order.DomainEvents.OfType<WorkOrderCompleted>().Single();
        evt.ProductId.Should().Be(Product);
        evt.QuantityProduced.Should().Be(10m);
        evt.Components.Should().HaveCount(2);
        evt.Components.Single(c => c.ComponentProductId == ComponentA).QuantityConsumed.Should().Be(20m);
    }

    [Fact]
    public void Complete_WhileDraft_Throws()
    {
        var order = Create();

        var act = () => order.Complete();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Release_WhenNotDraft_Throws()
    {
        var order = Create();
        order.Release();

        var act = () => order.Release();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_AfterCompletion_Throws()
    {
        var order = Create();
        order.Release();
        order.Complete();

        var act = () => order.Cancel();

        act.Should().Throw<InvalidOperationException>();
    }
}
