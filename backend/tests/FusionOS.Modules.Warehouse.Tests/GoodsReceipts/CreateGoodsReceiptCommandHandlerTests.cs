using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Commands.CreateGoodsReceipt;
using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Domain.GoodsReceipts;
using FluentAssertions;
using NSubstitute;
using Xunit;
using GoodsReceiptEntity = FusionOS.Modules.Warehouse.Domain.GoodsReceipts.GoodsReceipt;

namespace FusionOS.Modules.Warehouse.Tests.GoodsReceipts;

public class CreateGoodsReceiptCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenZoneExistsForWarehouse_PersistsReceiptAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var repository = Substitute.For<IGoodsReceiptRepository>();
        repository.ZoneExistsAsync(companyId, warehouseId, zoneId, Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateGoodsReceiptCommandHandler(repository, unitOfWork);
        var lines = new[] { new GoodsReceiptLineInput(productId, 10m, 5.5m) };
        var command = new CreateGoodsReceiptCommand(companyId, warehouseId, zoneId, null, null, lines);

        var result = await handler.Handle(command, CancellationToken.None);

        result.WarehouseId.Should().Be(warehouseId);
        result.ZoneId.Should().Be(zoneId);
        result.Lines.Should().ContainSingle(l => l.ProductId == productId && l.QuantityReceived == 10m);
        await repository.Received(1).AddAsync(Arg.Any<GoodsReceiptEntity>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenZoneDoesNotBelongToWarehouse_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var repository = Substitute.For<IGoodsReceiptRepository>();
        repository.ZoneExistsAsync(companyId, warehouseId, zoneId, Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateGoodsReceiptCommandHandler(repository, unitOfWork);
        var lines = new[] { new GoodsReceiptLineInput(Guid.NewGuid(), 10m, null) };
        var command = new CreateGoodsReceiptCommand(companyId, warehouseId, zoneId, null, null, lines);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<GoodsReceiptEntity>(), Arg.Any<CancellationToken>());
    }
}
