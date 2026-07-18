using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Application.Ledger.Queries.GetStockOnHand;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Ledger;

public class GetStockOnHandQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsTheSummedQuantityFromTheRepository()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var repository = Substitute.For<IInventoryLedgerRepository>();
        repository.SumQuantityAsync(companyId, productId, warehouseId, Arg.Any<CancellationToken>()).Returns(42.5m);
        var handler = new GetStockOnHandQueryHandler(repository);

        var result = await handler.Handle(new GetStockOnHandQuery(companyId, productId, warehouseId), CancellationToken.None);

        result.QuantityOnHand.Should().Be(42.5m);
        result.ProductId.Should().Be(productId);
        result.WarehouseId.Should().Be(warehouseId);
    }

    [Fact]
    public async Task Handle_WithNoWarehouseFilter_PassesNullThrough()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var repository = Substitute.For<IInventoryLedgerRepository>();
        repository.SumQuantityAsync(companyId, productId, null, Arg.Any<CancellationToken>()).Returns(10m);
        var handler = new GetStockOnHandQueryHandler(repository);

        var result = await handler.Handle(new GetStockOnHandQuery(companyId, productId, null), CancellationToken.None);

        result.QuantityOnHand.Should().Be(10m);
        await repository.Received(1).SumQuantityAsync(companyId, productId, null, Arg.Any<CancellationToken>());
    }
}
