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

    private static WorkOrder CreateWithOperation(out Guid operationId, decimal quantity = 10m)
    {
        var order = WorkOrder.Create(Company, Bom, Product, Warehouse, quantity, new[]
            {
                new BomComponentSnapshot(ComponentA, 2m),
            },
            new[]
            {
                new RoutingOperationSnapshot(Guid.NewGuid(), 1, "Cut", "WC-1", 30m),
            });

        operationId = order.Operations.Single().Id;
        return order;
    }

    [Fact]
    public void StartOperation_OnReleasedOrder_TransitionsPendingOperationToInProgress()
    {
        var order = CreateWithOperation(out var operationId);
        order.Release();
        var now = DateTimeOffset.UtcNow;

        order.StartOperation(operationId, now);

        var operation = order.Operations.Single(o => o.Id == operationId);
        operation.Status.Should().Be(WorkOrderOperationStatus.InProgress);
        operation.ActualStart.Should().Be(now);
    }

    [Fact]
    public void StartOperation_WhenOrderNotReleased_Throws()
    {
        var order = CreateWithOperation(out var operationId); // still Draft

        var act = () => order.StartOperation(operationId, DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void StartOperation_WhenAlreadyStarted_Throws()
    {
        var order = CreateWithOperation(out var operationId);
        order.Release();
        order.StartOperation(operationId, DateTimeOffset.UtcNow);

        var act = () => order.StartOperation(operationId, DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CompleteOperation_FromInProgress_TransitionsToCompleted()
    {
        var order = CreateWithOperation(out var operationId);
        order.Release();
        var start = DateTimeOffset.UtcNow;
        order.StartOperation(operationId, start);
        var end = start.AddHours(1);

        order.CompleteOperation(operationId, end);

        var operation = order.Operations.Single(o => o.Id == operationId);
        operation.Status.Should().Be(WorkOrderOperationStatus.Completed);
        operation.ActualEnd.Should().Be(end);
    }

    [Fact]
    public void CompleteOperation_WhenStillPending_Throws()
    {
        var order = CreateWithOperation(out var operationId);
        order.Release(); // operation itself was never started

        var act = () => order.CompleteOperation(operationId, DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CompleteOperation_WhenOrderNotReleased_Throws()
    {
        var order = CreateWithOperation(out var operationId); // Draft

        var act = () => order.CompleteOperation(operationId, DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ScheduleOperation_WhilePending_AssignsMachineAndWindow()
    {
        var order = CreateWithOperation(out var operationId);
        var machineId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var end = start.AddHours(4);

        order.ScheduleOperation(operationId, machineId, start, end);

        var operation = order.Operations.Single(o => o.Id == operationId);
        operation.MachineId.Should().Be(machineId);
        operation.ScheduledStart.Should().Be(start);
        operation.ScheduledEnd.Should().Be(end);
    }

    [Fact]
    public void ScheduleOperation_WhenOperationAlreadyStarted_Throws()
    {
        var order = CreateWithOperation(out var operationId);
        order.Release();
        order.StartOperation(operationId, DateTimeOffset.UtcNow);

        var act = () => order.ScheduleOperation(operationId, Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RecordRework_WithinRemainingScrap_AccumulatesQuantityReworked()
    {
        var order = Create(quantity: 10m);
        order.Release();
        order.Complete(quantityGoodProduced: 6m, quantityScrapped: 4m);

        order.RecordRework(3m);

        order.QuantityReworked.Should().Be(3m);
    }

    [Fact]
    public void RecordRework_ExceedingRemainingScrapQuantity_Throws()
    {
        var order = Create(quantity: 10m);
        order.Release();
        order.Complete(quantityGoodProduced: 6m, quantityScrapped: 4m);
        order.RecordRework(3m); // 1 unit of scrap remains unclaimed

        var act = () => order.RecordRework(2m); // would push total to 5, exceeding QuantityScrapped of 4

        act.Should().Throw<InvalidOperationException>();
        order.QuantityReworked.Should().Be(3m); // unchanged by the failed attempt
    }

    [Fact]
    public void RecordRework_WhenOrderNotCompleted_Throws()
    {
        var order = Create(quantity: 10m);
        order.Release(); // not yet Completed

        var act = () => order.RecordRework(1m);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RecordRework_ZeroOrNegativeQuantity_Throws()
    {
        var order = Create(quantity: 10m);
        order.Release();
        order.Complete(quantityGoodProduced: 8m, quantityScrapped: 2m);

        var act = () => order.RecordRework(0m);

        act.Should().Throw<ArgumentException>();
    }
}
