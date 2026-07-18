using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Application.Zones.Commands.DeactivateZone;
using FusionOS.Modules.Warehouse.Application.Zones.Contracts;
using FusionOS.Modules.Warehouse.Domain.Zones;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Zones;

public class DeactivateZoneCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenZoneBelongsToCompany_DeactivatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var zone = Zone.Create(companyId, Guid.NewGuid(), "Receiving Dock", "Z-01");
        var repository = Substitute.For<IZoneRepository>();
        repository.GetByIdAsync(zone.Id, Arg.Any<CancellationToken>()).Returns(zone);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateZoneCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateZoneCommand(companyId, zone.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenZoneBelongsToDifferentCompany_ThrowsValidationException()
    {
        var zone = Zone.Create(Guid.NewGuid(), Guid.NewGuid(), "Receiving Dock", "Z-01");
        var repository = Substitute.For<IZoneRepository>();
        repository.GetByIdAsync(zone.Id, Arg.Any<CancellationToken>()).Returns(zone);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateZoneCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new DeactivateZoneCommand(Guid.NewGuid(), zone.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenZoneDoesNotExist_ThrowsValidationException()
    {
        var repository = Substitute.For<IZoneRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Zone?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateZoneCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new DeactivateZoneCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
