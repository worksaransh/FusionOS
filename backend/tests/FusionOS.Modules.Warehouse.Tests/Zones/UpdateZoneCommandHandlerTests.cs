using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Application.Zones.Commands.UpdateZone;
using FusionOS.Modules.Warehouse.Application.Zones.Contracts;
using FusionOS.Modules.Warehouse.Domain.Zones;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Zones;

public class UpdateZoneCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenZoneBelongsToCompany_UpdatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var zone = Zone.Create(companyId, Guid.NewGuid(), "Old Name", "Z-01");
        var repository = Substitute.For<IZoneRepository>();
        repository.GetByIdAsync(zone.Id, Arg.Any<CancellationToken>()).Returns(zone);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateZoneCommandHandler(repository, unitOfWork);
        var command = new UpdateZoneCommand(companyId, zone.Id, "New Name");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("New Name");
        result.Code.Should().Be("Z-01");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenZoneBelongsToDifferentCompany_ThrowsValidationException()
    {
        var zone = Zone.Create(Guid.NewGuid(), Guid.NewGuid(), "Old Name", "Z-01");
        var repository = Substitute.For<IZoneRepository>();
        repository.GetByIdAsync(zone.Id, Arg.Any<CancellationToken>()).Returns(zone);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateZoneCommandHandler(repository, unitOfWork);
        var command = new UpdateZoneCommand(Guid.NewGuid(), zone.Id, "New Name");

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenZoneDoesNotExist_ThrowsValidationException()
    {
        var repository = Substitute.For<IZoneRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Zone?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateZoneCommandHandler(repository, unitOfWork);
        var command = new UpdateZoneCommand(Guid.NewGuid(), Guid.NewGuid(), "New Name");

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
