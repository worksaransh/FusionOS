using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Queries.ListWarehouses;
using FluentAssertions;
using NSubstitute;
using Xunit;
using WarehouseEntity = FusionOS.Modules.Warehouse.Domain.Warehouses.Warehouse;

namespace FusionOS.Modules.Warehouse.Tests.Warehouses;

public class ListWarehousesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedWarehousesForTheCompany()
    {
        var companyId = Guid.NewGuid();
        var warehouses = new[] { WarehouseEntity.Create(companyId, null, "Main DC", "WH-01", null) };
        var repository = Substitute.For<IWarehouseRepository>();
        repository.ListAsync(companyId, null, 1, 25, Arg.Any<CancellationToken>()).Returns(warehouses);
        repository.CountAsync(companyId, null, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListWarehousesQueryHandler(repository);

        var result = await handler.Handle(new ListWarehousesQuery(companyId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(w => w.Code == "WH-01");
    }

    [Fact]
    public async Task Handle_PassesSearchTermThroughToTheRepository()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IWarehouseRepository>();
        repository.ListAsync(companyId, "main", 1, 25, Arg.Any<CancellationToken>()).Returns(Array.Empty<WarehouseEntity>());
        repository.CountAsync(companyId, "main", Arg.Any<CancellationToken>()).Returns(0);
        var handler = new ListWarehousesQueryHandler(repository);

        await handler.Handle(new ListWarehousesQuery(companyId, "main"), CancellationToken.None);

        await repository.Received(1).ListAsync(companyId, "main", 1, 25, Arg.Any<CancellationToken>());
    }
}
