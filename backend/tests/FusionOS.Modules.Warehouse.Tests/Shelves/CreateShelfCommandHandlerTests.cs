using FusionOS.Modules.Warehouse.Application.Shelves.Commands.CreateShelf;
using FusionOS.Modules.Warehouse.Application.Shelves.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Domain.Shelves;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Shelves;

public class CreateShelfCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenRackExistsAndCodeUnique_PersistsShelf()
    {
        var repository = Substitute.For<IShelfRepository>();
        repository.RackExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateShelfCommandHandler(repository, unitOfWork);
        var command = new CreateShelfCommand(Guid.NewGuid(), Guid.NewGuid(), "Top Shelf", "S-01");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Code.Should().Be("S-01");
        await repository.Received(1).AddAsync(Arg.Any<Shelf>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRackDoesNotExist_Throws()
    {
        var repository = Substitute.For<IShelfRepository>();
        repository.RackExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateShelfCommandHandler(repository, unitOfWork);
        var command = new CreateShelfCommand(Guid.NewGuid(), Guid.NewGuid(), "Top Shelf", "S-01");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenCodeAlreadyExists_Throws()
    {
        var repository = Substitute.For<IShelfRepository>();
        repository.RackExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateShelfCommandHandler(repository, unitOfWork);
        var command = new CreateShelfCommand(Guid.NewGuid(), Guid.NewGuid(), "Top Shelf", "S-01");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
