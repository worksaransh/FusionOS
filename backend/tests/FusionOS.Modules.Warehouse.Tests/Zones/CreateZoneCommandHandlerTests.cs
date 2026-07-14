using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Application.Zones.Commands.CreateZone;
using FusionOS.Modules.Warehouse.Application.Zones.Contracts;
using FusionOS.Modules.Warehouse.Domain.Zones;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Zones;

public class CreateZoneCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenWarehouseExistsAndCodeUnique_PersistsZone()
    {
        var repository = Substitute.For<IZoneRepository>();
        repository.WarehouseExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateZoneCommandHandler(repository, unitOfWork);
        var command = new CreateZoneCommand(Guid.NewGuid(), Guid.NewGuid(), "Receiving Dock", "Z-01");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Code.Should().Be("Z-01");
        await repository.Received(1).AddAsync(Arg.Any<Zone>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWarehouseDoesNotExist_Throws()
    {
        var repository = Substitute.For<IZoneRepository>();
        repository.WarehouseExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateZoneCommandHandler(repository, unitOfWork);
        var command = new CreateZoneCommand(Guid.NewGuid(), Guid.NewGuid(), "Receiving Dock", "Z-01");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
