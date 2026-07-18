using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Queries.GetWarehouseById;
using FluentAssertions;
using NSubstitute;
using Xunit;
using WarehouseEntity = FusionOS.Modules.Warehouse.Domain.Warehouses.Warehouse;

namespace FusionOS.Modules.Warehouse.Tests.Warehouses;

public class GetWarehouseByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenWarehouseBelongsToCompany_ReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var warehouse = WarehouseEntity.Create(companyId, null, "Main DC", "WH-01", null);
        var repository = Substitute.For<IWarehouseRepository>();
        repository.GetByIdAsync(warehouse.Id, Arg.Any<CancellationToken>()).Returns(warehouse);
        var handler = new GetWarehouseByIdQueryHandler(repository);

        var result = await handler.Handle(new GetWarehouseByIdQuery(companyId, warehouse.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Code.Should().Be("WH-01");
    }

    [Fact]
    public async Task Handle_WhenWarehouseBelongsToDifferentCompany_ReturnsNull()
    {
        var warehouse = WarehouseEntity.Create(Guid.NewGuid(), null, "Main DC", "WH-01", null);
        var repository = Substitute.For<IWarehouseRepository>();
        repository.GetByIdAsync(warehouse.Id, Arg.Any<CancellationToken>()).Returns(warehouse);
        var handler = new GetWarehouseByIdQueryHandler(repository);

        var result = await handler.Handle(new GetWarehouseByIdQuery(Guid.NewGuid(), warehouse.Id), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenWarehouseDoesNotExist_ReturnsNull()
    {
        var repository = Substitute.For<IWarehouseRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((WarehouseEntity?)null);
        var handler = new GetWarehouseByIdQueryHandler(repository);

        var result = await handler.Handle(new GetWarehouseByIdQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}
