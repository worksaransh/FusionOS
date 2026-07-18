using FusionOS.Modules.Warehouse.Application.Bins.Commands.CreateBin;
using FusionOS.Modules.Warehouse.Application.Bins.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Domain.Bins;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Bins;

public class CreateBinCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenZoneExistsAndCodeUnique_PersistsBin()
    {
        var repository = Substitute.For<IBinRepository>();
        repository.ZoneExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateBinCommandHandler(repository, unitOfWork);
        var command = new CreateBinCommand(Guid.NewGuid(), Guid.NewGuid(), "Shelf 3", "A-01-03");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Code.Should().Be("A-01-03");
        await repository.Received(1).AddAsync(Arg.Any<Bin>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenZoneDoesNotExist_Throws()
    {
        var repository = Substitute.For<IBinRepository>();
        repository.ZoneExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateBinCommandHandler(repository, unitOfWork);
        var command = new CreateBinCommand(Guid.NewGuid(), Guid.NewGuid(), "Shelf 3", "A-01-03");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenCodeAlreadyExists_Throws()
    {
        var repository = Substitute.For<IBinRepository>();
        repository.ZoneExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateBinCommandHandler(repository, unitOfWork);
        var command = new CreateBinCommand(Guid.NewGuid(), Guid.NewGuid(), "Shelf 3", "A-01-03");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
