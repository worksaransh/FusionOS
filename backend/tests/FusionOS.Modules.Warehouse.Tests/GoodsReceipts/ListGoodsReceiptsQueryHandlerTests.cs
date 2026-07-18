using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Contracts;
using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Queries.ListGoodsReceipts;
using FusionOS.Modules.Warehouse.Domain.GoodsReceipts;
using FluentAssertions;
using NSubstitute;
using Xunit;
using GoodsReceiptEntity = FusionOS.Modules.Warehouse.Domain.GoodsReceipts.GoodsReceipt;

namespace FusionOS.Modules.Warehouse.Tests.GoodsReceipts;

public class ListGoodsReceiptsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedReceiptsForTheWarehouse()
    {
        var companyId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var lines = new[] { new GoodsReceiptLineInput(Guid.NewGuid(), 5m, 2m) };
        var receipts = new[] { GoodsReceiptEntity.Create(companyId, warehouseId, zoneId, null, null, lines) };
        var repository = Substitute.For<IGoodsReceiptRepository>();
        repository.ListAsync(companyId, warehouseId, 1, 25, Arg.Any<CancellationToken>()).Returns(receipts);
        repository.CountAsync(companyId, warehouseId, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListGoodsReceiptsQueryHandler(repository);

        var result = await handler.Handle(new ListGoodsReceiptsQuery(companyId, warehouseId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(r => r.WarehouseId == warehouseId);
    }
}
