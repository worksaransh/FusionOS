using FusionOS.Modules.Warehouse.Application.Zones.Contracts;
using FusionOS.Modules.Warehouse.Application.Zones.Queries.GetZoneById;
using FusionOS.Modules.Warehouse.Domain.Zones;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Zones;

public class GetZoneByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenZoneBelongsToCompany_ReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var zone = Zone.Create(companyId, Guid.NewGuid(), "Receiving Dock", "Z-01");
        var repository = Substitute.For<IZoneRepository>();
        repository.GetByIdAsync(zone.Id, Arg.Any<CancellationToken>()).Returns(zone);
        var handler = new GetZoneByIdQueryHandler(repository);

        var result = await handler.Handle(new GetZoneByIdQuery(companyId, zone.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Code.Should().Be("Z-01");
    }

    [Fact]
    public async Task Handle_WhenZoneBelongsToDifferentCompany_ReturnsNull()
    {
        var zone = Zone.Create(Guid.NewGuid(), Guid.NewGuid(), "Receiving Dock", "Z-01");
        var repository = Substitute.For<IZoneRepository>();
        repository.GetByIdAsync(zone.Id, Arg.Any<CancellationToken>()).Returns(zone);
        var handler = new GetZoneByIdQueryHandler(repository);

        var result = await handler.Handle(new GetZoneByIdQuery(Guid.NewGuid(), zone.Id), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenZoneDoesNotExist_ReturnsNull()
    {
        var repository = Substitute.For<IZoneRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Zone?)null);
        var handler = new GetZoneByIdQueryHandler(repository);

        var result = await handler.Handle(new GetZoneByIdQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}
