using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Bins.Commands.AssignBinShelf;
using FusionOS.Modules.Warehouse.Application.Bins.Contracts;
using FusionOS.Modules.Warehouse.Application.Racks.Contracts;
using FusionOS.Modules.Warehouse.Application.Shelves.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Domain.Bins;
using FusionOS.Modules.Warehouse.Domain.Racks;
using FusionOS.Modules.Warehouse.Domain.Shelves;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Bins;

public class AssignBinShelfCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenShelfsRackZoneMatchesBinZone_AssignsShelf()
    {
        var companyId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var bin = Bin.Create(companyId, zoneId, "Shelf 3", "A-01-03");
        var rack = Rack.Create(companyId, zoneId, "Aisle 1", "R-01");
        var shelf = Shelf.Create(companyId, rack.Id, "Top Shelf", "S-01");

        var binRepository = Substitute.For<IBinRepository>();
        binRepository.GetByIdAsync(bin.Id, Arg.Any<CancellationToken>()).Returns(bin);
        var shelfRepository = Substitute.For<IShelfRepository>();
        shelfRepository.GetByIdAsync(shelf.Id, Arg.Any<CancellationToken>()).Returns(shelf);
        var rackRepository = Substitute.For<IRackRepository>();
        rackRepository.GetByIdAsync(rack.Id, Arg.Any<CancellationToken>()).Returns(rack);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AssignBinShelfCommandHandler(binRepository, shelfRepository, rackRepository, unitOfWork);
        var command = new AssignBinShelfCommand(companyId, bin.Id, shelf.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.ShelfId.Should().Be(shelf.Id);
        bin.ShelfId.Should().Be(shelf.Id);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenShelfsRackBelongsToDifferentZoneThanBin_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var binZoneId = Guid.NewGuid();
        var otherZoneId = Guid.NewGuid();
        var bin = Bin.Create(companyId, binZoneId, "Shelf 3", "A-01-03");
        var rack = Rack.Create(companyId, otherZoneId, "Aisle 1", "R-01");
        var shelf = Shelf.Create(companyId, rack.Id, "Top Shelf", "S-01");

        var binRepository = Substitute.For<IBinRepository>();
        binRepository.GetByIdAsync(bin.Id, Arg.Any<CancellationToken>()).Returns(bin);
        var shelfRepository = Substitute.For<IShelfRepository>();
        shelfRepository.GetByIdAsync(shelf.Id, Arg.Any<CancellationToken>()).Returns(shelf);
        var rackRepository = Substitute.For<IRackRepository>();
        rackRepository.GetByIdAsync(rack.Id, Arg.Any<CancellationToken>()).Returns(rack);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AssignBinShelfCommandHandler(binRepository, shelfRepository, rackRepository, unitOfWork);
        var command = new AssignBinShelfCommand(companyId, bin.Id, shelf.Id);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        bin.ShelfId.Should().BeNull();
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenShelfIdIsNull_ClearsShelf()
    {
        var companyId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var bin = Bin.Create(companyId, zoneId, "Shelf 3", "A-01-03");
        bin.AssignShelf(Guid.NewGuid());

        var binRepository = Substitute.For<IBinRepository>();
        binRepository.GetByIdAsync(bin.Id, Arg.Any<CancellationToken>()).Returns(bin);
        var shelfRepository = Substitute.For<IShelfRepository>();
        var rackRepository = Substitute.For<IRackRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AssignBinShelfCommandHandler(binRepository, shelfRepository, rackRepository, unitOfWork);
        var command = new AssignBinShelfCommand(companyId, bin.Id, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.ShelfId.Should().BeNull();
        bin.ShelfId.Should().BeNull();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenBinDoesNotExist_ThrowsValidationException()
    {
        var binRepository = Substitute.For<IBinRepository>();
        binRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Bin?)null);
        var shelfRepository = Substitute.For<IShelfRepository>();
        var rackRepository = Substitute.For<IRackRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AssignBinShelfCommandHandler(binRepository, shelfRepository, rackRepository, unitOfWork);
        var command = new AssignBinShelfCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenShelfDoesNotExist_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var bin = Bin.Create(companyId, Guid.NewGuid(), "Shelf 3", "A-01-03");

        var binRepository = Substitute.For<IBinRepository>();
        binRepository.GetByIdAsync(bin.Id, Arg.Any<CancellationToken>()).Returns(bin);
        var shelfRepository = Substitute.For<IShelfRepository>();
        shelfRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Shelf?)null);
        var rackRepository = Substitute.For<IRackRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AssignBinShelfCommandHandler(binRepository, shelfRepository, rackRepository, unitOfWork);
        var command = new AssignBinShelfCommand(companyId, bin.Id, Guid.NewGuid());

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
