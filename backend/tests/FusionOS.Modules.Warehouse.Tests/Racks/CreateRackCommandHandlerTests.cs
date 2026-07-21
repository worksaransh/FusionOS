using FusionOS.Modules.Warehouse.Application.Racks.Commands.CreateRack;
using FusionOS.Modules.Warehouse.Application.Racks.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Domain.Racks;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Racks;

public class CreateRackCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenZoneExistsAndCodeUnique_PersistsRack()
    {
        var repository = Substitute.For<IRackRepository>();
        repository.ZoneExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateRackCommandHandler(repository, unitOfWork);
        var command = new CreateRackCommand(Guid.NewGuid(), Guid.NewGuid(), "Aisle 3", "R-01");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Code.Should().Be("R-01");
        await repository.Received(1).AddAsync(Arg.Any<Rack>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenZoneDoesNotExist_Throws()
    {
        var repository = Substitute.For<IRackRepository>();
        repository.ZoneExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateRackCommandHandler(repository, unitOfWork);
        var command = new CreateRackCommand(Guid.NewGuid(), Guid.NewGuid(), "Aisle 3", "R-01");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenCodeAlreadyExists_Throws()
    {
        var repository = Substitute.For<IRackRepository>();
        repository.ZoneExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateRackCommandHandler(repository, unitOfWork);
        var command = new CreateRackCommand(Guid.NewGuid(), Guid.NewGuid(), "Aisle 3", "R-01");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
