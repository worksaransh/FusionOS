using FusionOS.Modules.Warehouse.Application.Racks.Contracts;
using FusionOS.Modules.Warehouse.Application.Racks.Queries.ListRacks;
using FusionOS.Modules.Warehouse.Domain.Racks;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Racks;

public class ListRacksQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedRacksForTheZone()
    {
        var companyId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var racks = new[] { Rack.Create(companyId, zoneId, "Aisle 3", "R-01") };
        var repository = Substitute.For<IRackRepository>();
        repository.ListAsync(companyId, zoneId, 1, 25, Arg.Any<CancellationToken>()).Returns(racks);
        repository.CountAsync(companyId, zoneId, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListRacksQueryHandler(repository);

        var result = await handler.Handle(new ListRacksQuery(companyId, zoneId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(r => r.Code == "R-01");
    }

    [Fact]
    public async Task Handle_ScopesToTheGivenZoneId()
    {
        var companyId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var repository = Substitute.For<IRackRepository>();
        repository.ListAsync(companyId, zoneId, 1, 25, Arg.Any<CancellationToken>()).Returns(Array.Empty<Rack>());
        repository.CountAsync(companyId, zoneId, Arg.Any<CancellationToken>()).Returns(0);
        var handler = new ListRacksQueryHandler(repository);

        await handler.Handle(new ListRacksQuery(companyId, zoneId), CancellationToken.None);

        await repository.Received(1).ListAsync(companyId, zoneId, 1, 25, Arg.Any<CancellationToken>());
    }
}
