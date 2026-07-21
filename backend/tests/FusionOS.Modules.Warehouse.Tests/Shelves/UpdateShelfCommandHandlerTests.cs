using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Shelves.Commands.UpdateShelf;
using FusionOS.Modules.Warehouse.Application.Shelves.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Domain.Shelves;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Shelves;

public class UpdateShelfCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenShelfBelongsToCompany_UpdatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var shelf = Shelf.Create(companyId, Guid.NewGuid(), "Old Name", "S-01");
        var repository = Substitute.For<IShelfRepository>();
        repository.GetByIdAsync(shelf.Id, Arg.Any<CancellationToken>()).Returns(shelf);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateShelfCommandHandler(repository, unitOfWork);
        var command = new UpdateShelfCommand(companyId, shelf.Id, "New Name");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("New Name");
        result.Code.Should().Be("S-01");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenShelfBelongsToDifferentCompany_ThrowsValidationException()
    {
        var shelf = Shelf.Create(Guid.NewGuid(), Guid.NewGuid(), "Old Name", "S-01");
        var repository = Substitute.For<IShelfRepository>();
        repository.GetByIdAsync(shelf.Id, Arg.Any<CancellationToken>()).Returns(shelf);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateShelfCommandHandler(repository, unitOfWork);
        var command = new UpdateShelfCommand(Guid.NewGuid(), shelf.Id, "New Name");

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenShelfDoesNotExist_ThrowsValidationException()
    {
        var repository = Substitute.For<IShelfRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Shelf?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateShelfCommandHandler(repository, unitOfWork);
        var command = new UpdateShelfCommand(Guid.NewGuid(), Guid.NewGuid(), "New Name");

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
