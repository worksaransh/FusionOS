using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Shelves.Commands.DeactivateShelf;
using FusionOS.Modules.Warehouse.Application.Shelves.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Domain.Shelves;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Shelves;

public class DeactivateShelfCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenShelfBelongsToCompany_DeactivatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var shelf = Shelf.Create(companyId, Guid.NewGuid(), "Top Shelf", "S-01");
        var repository = Substitute.For<IShelfRepository>();
        repository.GetByIdAsync(shelf.Id, Arg.Any<CancellationToken>()).Returns(shelf);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateShelfCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateShelfCommand(companyId, shelf.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenShelfBelongsToDifferentCompany_ThrowsValidationException()
    {
        var shelf = Shelf.Create(Guid.NewGuid(), Guid.NewGuid(), "Top Shelf", "S-01");
        var repository = Substitute.For<IShelfRepository>();
        repository.GetByIdAsync(shelf.Id, Arg.Any<CancellationToken>()).Returns(shelf);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateShelfCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new DeactivateShelfCommand(Guid.NewGuid(), shelf.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenShelfDoesNotExist_ThrowsValidationException()
    {
        var repository = Substitute.For<IShelfRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Shelf?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateShelfCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new DeactivateShelfCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
