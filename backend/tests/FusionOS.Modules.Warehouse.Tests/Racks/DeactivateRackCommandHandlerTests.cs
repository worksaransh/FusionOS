using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Racks.Commands.DeactivateRack;
using FusionOS.Modules.Warehouse.Application.Racks.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Domain.Racks;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Racks;

public class DeactivateRackCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenRackBelongsToCompany_DeactivatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var rack = Rack.Create(companyId, Guid.NewGuid(), "Aisle 3", "R-01");
        var repository = Substitute.For<IRackRepository>();
        repository.GetByIdAsync(rack.Id, Arg.Any<CancellationToken>()).Returns(rack);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateRackCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateRackCommand(companyId, rack.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRackBelongsToDifferentCompany_ThrowsValidationException()
    {
        var rack = Rack.Create(Guid.NewGuid(), Guid.NewGuid(), "Aisle 3", "R-01");
        var repository = Substitute.For<IRackRepository>();
        repository.GetByIdAsync(rack.Id, Arg.Any<CancellationToken>()).Returns(rack);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateRackCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new DeactivateRackCommand(Guid.NewGuid(), rack.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenRackDoesNotExist_ThrowsValidationException()
    {
        var repository = Substitute.For<IRackRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Rack?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateRackCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new DeactivateRackCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
