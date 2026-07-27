using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.SupplierRatings.Commands.CreateSupplierRating;
using FusionOS.Modules.Procurement.Application.SupplierRatings.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.Modules.Procurement.Domain.PurchaseOrders;
using FusionOS.Modules.Procurement.Domain.SupplierRatings;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.SupplierRatings;

public class CreateSupplierRatingCommandHandlerTests
{
    private static PurchaseOrder CreateFullyReceivedOrder(Guid companyId, Guid supplierId, Guid productId)
    {
        var order = PurchaseOrder.Create(companyId, supplierId, new[] { new PurchaseOrderLineInput(productId, 10m, 5m) });
        order.RecordGoodsReceipt(productId, 10m);
        return order;
    }

    private static PurchaseOrder CreateApprovedOrder(Guid companyId, Guid supplierId, Guid productId)
    {
        var order = PurchaseOrder.Create(companyId, supplierId, new[] { new PurchaseOrderLineInput(productId, 10m, 5m) });
        order.Approve();
        return order;
    }

    [Fact]
    public async Task Handle_WithNoPurchaseOrder_PersistsRating()
    {
        var repository = Substitute.For<ISupplierRatingRepository>();
        var supplierRepository = Substitute.For<ISupplierRepository>();
        supplierRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var purchaseOrderRepository = Substitute.For<IPurchaseOrderRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateSupplierRatingCommandHandler(repository, supplierRepository, purchaseOrderRepository, unitOfWork);
        var command = new CreateSupplierRatingCommand(Guid.NewGuid(), Guid.NewGuid(), null, 5, 4, Guid.NewGuid(), "Good.");

        var result = await handler.Handle(command, CancellationToken.None);

        result.QualityScore.Should().Be(5);
        result.DeliveryScore.Should().Be(4);
        result.PurchaseOrderId.Should().BeNull();
        await repository.Received(1).AddAsync(Arg.Any<SupplierRating>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSupplierDoesNotExist_Throws()
    {
        var repository = Substitute.For<ISupplierRatingRepository>();
        var supplierRepository = Substitute.For<ISupplierRepository>();
        supplierRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var purchaseOrderRepository = Substitute.For<IPurchaseOrderRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateSupplierRatingCommandHandler(repository, supplierRepository, purchaseOrderRepository, unitOfWork);
        var command = new CreateSupplierRatingCommand(Guid.NewGuid(), Guid.NewGuid(), null, 5, 4, Guid.NewGuid(), null);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<SupplierRating>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithFullyReceivedPurchaseOrderForSameSupplier_PersistsRating()
    {
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var order = CreateFullyReceivedOrder(companyId, supplierId, Guid.NewGuid());

        var repository = Substitute.For<ISupplierRatingRepository>();
        var supplierRepository = Substitute.For<ISupplierRepository>();
        supplierRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var purchaseOrderRepository = Substitute.For<IPurchaseOrderRepository>();
        purchaseOrderRepository.GetByIdAsync(companyId, order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateSupplierRatingCommandHandler(repository, supplierRepository, purchaseOrderRepository, unitOfWork);
        var command = new CreateSupplierRatingCommand(companyId, supplierId, order.Id, 5, 5, Guid.NewGuid(), null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.PurchaseOrderId.Should().Be(order.Id);
        await repository.Received(1).AddAsync(Arg.Any<SupplierRating>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithPurchaseOrderNotFullyReceived_Throws()
    {
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var order = CreateApprovedOrder(companyId, supplierId, Guid.NewGuid());

        var repository = Substitute.For<ISupplierRatingRepository>();
        var supplierRepository = Substitute.For<ISupplierRepository>();
        supplierRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var purchaseOrderRepository = Substitute.For<IPurchaseOrderRepository>();
        purchaseOrderRepository.GetByIdAsync(companyId, order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateSupplierRatingCommandHandler(repository, supplierRepository, purchaseOrderRepository, unitOfWork);
        var command = new CreateSupplierRatingCommand(companyId, supplierId, order.Id, 5, 5, Guid.NewGuid(), null);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<SupplierRating>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithPurchaseOrderBelongingToDifferentSupplier_Throws()
    {
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var otherSupplierId = Guid.NewGuid();
        var order = CreateFullyReceivedOrder(companyId, otherSupplierId, Guid.NewGuid());

        var repository = Substitute.For<ISupplierRatingRepository>();
        var supplierRepository = Substitute.For<ISupplierRepository>();
        supplierRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var purchaseOrderRepository = Substitute.For<IPurchaseOrderRepository>();
        purchaseOrderRepository.GetByIdAsync(companyId, order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateSupplierRatingCommandHandler(repository, supplierRepository, purchaseOrderRepository, unitOfWork);
        var command = new CreateSupplierRatingCommand(companyId, supplierId, order.Id, 5, 5, Guid.NewGuid(), null);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<SupplierRating>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentPurchaseOrder_Throws()
    {
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();

        var repository = Substitute.For<ISupplierRatingRepository>();
        var supplierRepository = Substitute.For<ISupplierRepository>();
        supplierRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var purchaseOrderRepository = Substitute.For<IPurchaseOrderRepository>();
        purchaseOrderRepository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((PurchaseOrder?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateSupplierRatingCommandHandler(repository, supplierRepository, purchaseOrderRepository, unitOfWork);
        var command = new CreateSupplierRatingCommand(companyId, supplierId, Guid.NewGuid(), 5, 5, Guid.NewGuid(), null);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<SupplierRating>(), Arg.Any<CancellationToken>());
    }
}
