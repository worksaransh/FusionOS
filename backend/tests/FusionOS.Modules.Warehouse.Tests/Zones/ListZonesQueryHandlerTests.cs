using FusionOS.Modules.Warehouse.Application.Zones.Contracts;
using FusionOS.Modules.Warehouse.Application.Zones.Queries.ListZones;
using FusionOS.Modules.Warehouse.Domain.Zones;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Zones;

public class ListZonesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedZonesForTheWarehouse()
    {
        var companyId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var zones = new[] { Zone.Create(companyId, warehouseId, "Receiving Dock", "Z-01") };
        var repository = Substitute.For<IZoneRepository>();
        repository.ListAsync(companyId, warehouseId, 1, 25, Arg.Any<CancellationToken>()).Returns(zones);
        repository.CountAsync(companyId, warehouseId, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListZonesQueryHandler(repository);

        var result = await handler.Handle(new ListZonesQuery(companyId, warehouseId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(z => z.Code == "Z-01");
    }

    [Fact]
    public async Task Handle_ScopesToTheGivenWarehouseId()
    {
        var companyId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var repository = Substitute.For<IZoneRepository>();
        repository.ListAsync(companyId, warehouseId, 1, 25, Arg.Any<CancellationToken>()).Returns(Array.Empty<Zone>());
        repository.CountAsync(companyId, warehouseId, Arg.Any<CancellationToken>()).Returns(0);
        var handler = new ListZonesQueryHandler(repository);

        await handler.Handle(new ListZonesQuery(companyId, warehouseId), CancellationToken.None);

        await repository.Received(1).ListAsync(companyId, warehouseId, 1, 25, Arg.Any<CancellationToken>());
    }
}
