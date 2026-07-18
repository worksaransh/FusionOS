using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.CompleteWorkOrder;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.CreateWorkOrder;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;
using FusionOS.Modules.Manufacturing.Domain.BillOfMaterials;
using FusionOS.Modules.Manufacturing.Domain.WorkOrders;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Manufacturing.Tests.WorkOrders;

public class WorkOrderCommandHandlerTests
{
    private static Domain.BillOfMaterials.BillOfMaterials BuildBom(Guid companyId, Guid componentId) =>
        Domain.BillOfMaterials.BillOfMaterials.Create(companyId, "WIDGET-A", "Widget A", Guid.NewGuid(), new[] { new BomLineInput(componentId, 3m) });

    [Fact]
    public async Task CreateWorkOrder_SnapshotsBomComponentsScaledByQuantity()
    {
        var companyId = Guid.NewGuid();
        var componentId = Guid.NewGuid();
        var bom = BuildBom(companyId, componentId);

        var bomRepository = Substitute.For<IBillOfMaterialsRepository>();
        bomRepository.GetByIdAsync(companyId, bom.Id, Arg.Any<CancellationToken>()).Returns(bom);
        var workOrderRepository = Substitute.For<IWorkOrderRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateWorkOrderCommandHandler(workOrderRepository, bomRepository, unitOfWork);

        var result = await handler.Handle(new CreateWorkOrderCommand(companyId, bom.Id, Guid.NewGuid(), 4m), CancellationToken.None);

        result.Status.Should().Be("Draft");
        result.QuantityToProduce.Should().Be(4m);
        result.Components.Single(c => c.ComponentProductId == componentId).QuantityRequired.Should().Be(12m); // 3 * 4
        await workOrderRepository.Received(1).AddAsync(Arg.Any<WorkOrder>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateWorkOrder_WhenBomMissing_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var bomRepository = Substitute.For<IBillOfMaterialsRepository>();
        bomRepository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Domain.BillOfMaterials.BillOfMaterials?)null);
        var handler = new CreateWorkOrderCommandHandler(Substitute.For<IWorkOrderRepository>(), bomRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new CreateWorkOrderCommand(companyId, Guid.NewGuid(), Guid.NewGuid(), 1m), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CompleteWorkOrder_MovesReleasedOrderToCompleted()
    {
        var companyId = Guid.NewGuid();
        var order = WorkOrder.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5m,
            new[] { new BomComponentSnapshot(Guid.NewGuid(), 1m) });
        order.Release();

        var repository = Substitute.For<IWorkOrderRepository>();
        repository.GetByIdAsync(companyId, order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CompleteWorkOrderCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new CompleteWorkOrderCommand(companyId, order.Id), CancellationToken.None);

        result.Status.Should().Be("Completed");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
