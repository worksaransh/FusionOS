using FusionOS.Modules.Warehouse.Application.Bins.Contracts;
using FusionOS.Modules.Warehouse.Application.PickLists.Commands.CreatePickList;
using FusionOS.Modules.Warehouse.Application.PickLists.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Application.Zones.Contracts;
using FusionOS.Modules.Warehouse.Domain.PickLists;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.PickLists;

public class CreatePickListCommandHandlerTests
{
    private static CreatePickListCommand BuildCommand(Guid? binId = null) => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
        new[] { new PickListLineInput(Guid.NewGuid(), binId, 5m) });

    [Fact]
    public async Task Handle_WhenWarehouseExistsAndNoBinRequested_PersistsPickList()
    {
        var repository = Substitute.For<IPickListRepository>();
        var zoneRepository = Substitute.For<IZoneRepository>();
        zoneRepository.WarehouseExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var binRepository = Substitute.For<IBinRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreatePickListCommandHandler(repository, zoneRepository, binRepository, unitOfWork);
        var command = BuildCommand();

        var result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(nameof(PickListStatus.Pending));
        await repository.Received(1).AddAsync(Arg.Any<Domain.PickLists.PickList>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWarehouseDoesNotExist_Throws()
    {
        var repository = Substitute.For<IPickListRepository>();
        var zoneRepository = Substitute.For<IZoneRepository>();
        zoneRepository.WarehouseExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var binRepository = Substitute.For<IBinRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreatePickListCommandHandler(repository, zoneRepository, binRepository, unitOfWork);
        var command = BuildCommand();

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenLineBinDoesNotExist_Throws()
    {
        var repository = Substitute.For<IPickListRepository>();
        var zoneRepository = Substitute.For<IZoneRepository>();
        zoneRepository.WarehouseExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var binRepository = Substitute.For<IBinRepository>();
        binRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreatePickListCommandHandler(repository, zoneRepository, binRepository, unitOfWork);
        var command = BuildCommand(Guid.NewGuid());

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
