using FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.Modules.Procurement.Domain.PurchaseOrders;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.PurchaseOrders;

public class CreatePurchaseOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenSupplierExists_PersistsOrder()
    {
        var repository = Substitute.For<IPurchaseOrderRepository>();
        var supplierRepository = Substitute.For<ISupplierRepository>();
        supplierRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreatePurchaseOrderCommandHandler(repository, supplierRepository, unitOfWork);
        var command = new CreatePurchaseOrderCommand(Guid.NewGuid(), Guid.NewGuid(), new[] { new PurchaseOrderLineInput(Guid.NewGuid(), 5m, 10m) });

        var result = await handler.Handle(command, CancellationToken.None);

        result.TotalAmount.Should().Be(50m);
        await repository.Received(1).AddAsync(Arg.Any<FusionOS.Modules.Procurement.Domain.PurchaseOrders.PurchaseOrder>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSupplierDoesNotExist_Throws()
    {
        var repository = Substitute.For<IPurchaseOrderRepository>();
        var supplierRepository = Substitute.For<ISupplierRepository>();
        supplierRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreatePurchaseOrderCommandHandler(repository, supplierRepository, unitOfWork);
        var command = new CreatePurchaseOrderCommand(Guid.NewGuid(), Guid.NewGuid(), new[] { new PurchaseOrderLineInput(Guid.NewGuid(), 5m, 10m) });

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FusionOS.BuildingBlocks.Application.Exceptions.ValidationException>();
    }
}
