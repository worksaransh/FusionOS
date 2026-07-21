using FluentAssertions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.CompleteWorkOrder;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.IssueMaterialToWorkOrder;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.ReturnMaterialFromWorkOrder;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;
using FusionOS.Modules.Manufacturing.Domain.WorkOrders;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Manufacturing.Tests.WorkOrders;

public class WorkOrderExtensionsCommandHandlerTests
{
    private static readonly Guid ComponentId = Guid.NewGuid();

    private static WorkOrder BuildReleasedOrder(Guid companyId, decimal quantityToProduce = 5m)
    {
        var order = WorkOrder.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), quantityToProduce,
            new[] { new BomComponentSnapshot(ComponentId, 2m) });
        order.Release();
        return order;
    }

    [Fact]
    public async Task CompleteWorkOrder_WithScrapAndYield_ReturnsThemOnDto()
    {
        var companyId = Guid.NewGuid();
        var order = BuildReleasedOrder(companyId, quantityToProduce: 10m);
        var repository = Substitute.For<IWorkOrderRepository>();
        repository.GetByIdAsync(companyId, order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CompleteWorkOrderCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new CompleteWorkOrderCommand(companyId, order.Id, QuantityGoodProduced: 8m, QuantityScrapped: 2m), CancellationToken.None);

        result.Status.Should().Be("Completed");
        result.QuantityGoodProduced.Should().Be(8m);
        result.QuantityScrapped.Should().Be(2m);
        result.YieldPercentage.Should().Be(80m);
    }

    [Fact]
    public async Task IssueMaterialToWorkOrder_UpdatesComponentQuantityIssued()
    {
        var companyId = Guid.NewGuid();
        var order = BuildReleasedOrder(companyId, quantityToProduce: 5m); // QuantityRequired = 2 * 5 = 10
        var repository = Substitute.For<IWorkOrderRepository>();
        repository.GetByIdAsync(companyId, order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new IssueMaterialToWorkOrderCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new IssueMaterialToWorkOrderCommand(companyId, order.Id, ComponentId, 6m), CancellationToken.None);

        result.Components.Single(c => c.ComponentProductId == ComponentId).QuantityIssued.Should().Be(6m);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IssueMaterialToWorkOrder_WhenWorkOrderMissing_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IWorkOrderRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((WorkOrder?)null);
        var handler = new IssueMaterialToWorkOrderCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new IssueMaterialToWorkOrderCommand(companyId, Guid.NewGuid(), ComponentId, 1m), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ReturnMaterialFromWorkOrder_ReducesComponentQuantityIssued()
    {
        var companyId = Guid.NewGuid();
        var order = BuildReleasedOrder(companyId, quantityToProduce: 5m);
        order.IssueMaterial(ComponentId, 6m);
        var repository = Substitute.For<IWorkOrderRepository>();
        repository.GetByIdAsync(companyId, order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ReturnMaterialFromWorkOrderCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new ReturnMaterialFromWorkOrderCommand(companyId, order.Id, ComponentId, 4m), CancellationToken.None);

        result.Components.Single(c => c.ComponentProductId == ComponentId).QuantityIssued.Should().Be(2m);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
