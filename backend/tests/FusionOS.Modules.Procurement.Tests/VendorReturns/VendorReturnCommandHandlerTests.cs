using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.VendorReturns.Commands.CancelVendorReturn;
using FusionOS.Modules.Procurement.Application.VendorReturns.Commands.CompleteVendorReturn;
using FusionOS.Modules.Procurement.Application.VendorReturns.Commands.CreateVendorReturn;
using FusionOS.Modules.Procurement.Application.VendorReturns.Contracts;
using FusionOS.Modules.Procurement.Domain.PurchaseOrders;
using FusionOS.Modules.Procurement.Domain.VendorReturns;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.VendorReturns;

public class VendorReturnCommandHandlerTests
{
    private static PurchaseOrder CreateReceivedPurchaseOrder(Guid companyId, Guid productId, decimal orderedQuantity, decimal receivedQuantity)
    {
        var order = PurchaseOrder.Create(companyId, Guid.NewGuid(), new[] { new PurchaseOrderLineInput(productId, orderedQuantity, 10m, null, 0m) });
        order.RecordGoodsReceipt(productId, receivedQuantity);
        return order;
    }

    [Fact]
    public async Task CreateVendorReturn_WithinReceivedQuantity_PersistsPendingReturn()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var purchaseOrder = CreateReceivedPurchaseOrder(companyId, productId, 10m, 10m);
        var purchaseOrderRepository = Substitute.For<IPurchaseOrderRepository>();
        purchaseOrderRepository.GetByIdAsync(companyId, purchaseOrder.Id, Arg.Any<CancellationToken>()).Returns(purchaseOrder);
        var repository = Substitute.For<IVendorReturnRepository>();
        repository.SumReturnedQuantityAsync(companyId, purchaseOrder.Id, productId, Arg.Any<CancellationToken>()).Returns(0m);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateVendorReturnCommandHandler(repository, purchaseOrderRepository, unitOfWork);

        var result = await handler.Handle(
            new CreateVendorReturnCommand(companyId, purchaseOrder.Id, productId, Guid.NewGuid(), 4m, "Damaged in transit"),
            CancellationToken.None);

        result.Status.Should().Be("Pending");
        await repository.Received(1).AddAsync(Arg.Any<VendorReturn>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateVendorReturn_ExceedingReturnableQuantity_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var purchaseOrder = CreateReceivedPurchaseOrder(companyId, productId, 10m, 10m);
        var purchaseOrderRepository = Substitute.For<IPurchaseOrderRepository>();
        purchaseOrderRepository.GetByIdAsync(companyId, purchaseOrder.Id, Arg.Any<CancellationToken>()).Returns(purchaseOrder);
        var repository = Substitute.For<IVendorReturnRepository>();
        repository.SumReturnedQuantityAsync(companyId, purchaseOrder.Id, productId, Arg.Any<CancellationToken>()).Returns(8m);
        var handler = new CreateVendorReturnCommandHandler(repository, purchaseOrderRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(
            new CreateVendorReturnCommand(companyId, purchaseOrder.Id, productId, Guid.NewGuid(), 5m, "Damaged in transit"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateVendorReturn_ForProductNotOnPurchaseOrder_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var purchaseOrder = CreateReceivedPurchaseOrder(companyId, Guid.NewGuid(), 10m, 10m);
        var purchaseOrderRepository = Substitute.For<IPurchaseOrderRepository>();
        purchaseOrderRepository.GetByIdAsync(companyId, purchaseOrder.Id, Arg.Any<CancellationToken>()).Returns(purchaseOrder);
        var handler = new CreateVendorReturnCommandHandler(Substitute.For<IVendorReturnRepository>(), purchaseOrderRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(
            new CreateVendorReturnCommand(companyId, purchaseOrder.Id, Guid.NewGuid(), Guid.NewGuid(), 1m, "Damaged"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateVendorReturn_WhenPurchaseOrderDoesNotExist_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var purchaseOrderRepository = Substitute.For<IPurchaseOrderRepository>();
        purchaseOrderRepository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((PurchaseOrder?)null);
        var handler = new CreateVendorReturnCommandHandler(Substitute.For<IVendorReturnRepository>(), purchaseOrderRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(
            new CreateVendorReturnCommand(companyId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1m, "Damaged"),
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CompleteVendorReturn_ResolvesToCompleted()
    {
        var companyId = Guid.NewGuid();
        var vendorReturn = VendorReturn.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 4m, "Damaged");
        var repository = Substitute.For<IVendorReturnRepository>();
        repository.GetByIdAsync(companyId, vendorReturn.Id, Arg.Any<CancellationToken>()).Returns(vendorReturn);
        var handler = new CompleteVendorReturnCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new CompleteVendorReturnCommand(companyId, vendorReturn.Id), CancellationToken.None);

        result.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task CancelVendorReturn_ResolvesToCancelled()
    {
        var companyId = Guid.NewGuid();
        var vendorReturn = VendorReturn.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 4m, "Damaged");
        var repository = Substitute.For<IVendorReturnRepository>();
        repository.GetByIdAsync(companyId, vendorReturn.Id, Arg.Any<CancellationToken>()).Returns(vendorReturn);
        var handler = new CancelVendorReturnCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new CancelVendorReturnCommand(companyId, vendorReturn.Id), CancellationToken.None);

        result.Status.Should().Be("Cancelled");
    }
}
