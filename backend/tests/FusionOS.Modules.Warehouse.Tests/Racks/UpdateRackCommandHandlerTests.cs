using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Racks.Commands.UpdateRack;
using FusionOS.Modules.Warehouse.Application.Racks.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Domain.Racks;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.Racks;

public class UpdateRackCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenRackBelongsToCompany_UpdatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var rack = Rack.Create(companyId, Guid.NewGuid(), "Old Name", "R-01");
        var repository = Substitute.For<IRackRepository>();
        repository.GetByIdAsync(rack.Id, Arg.Any<CancellationToken>()).Returns(rack);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateRackCommandHandler(repository, unitOfWork);
        var command = new UpdateRackCommand(companyId, rack.Id, "New Name");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("New Name");
        result.Code.Should().Be("R-01");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRackBelongsToDifferentCompany_ThrowsValidationException()
    {
        var rack = Rack.Create(Guid.NewGuid(), Guid.NewGuid(), "Old Name", "R-01");
        var repository = Substitute.For<IRackRepository>();
        repository.GetByIdAsync(rack.Id, Arg.Any<CancellationToken>()).Returns(rack);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateRackCommandHandler(repository, unitOfWork);
        var command = new UpdateRackCommand(Guid.NewGuid(), rack.Id, "New Name");

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenRackDoesNotExist_ThrowsValidationException()
    {
        var repository = Substitute.For<IRackRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Rack?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateRackCommandHandler(repository, unitOfWork);
        var command = new UpdateRackCommand(Guid.NewGuid(), Guid.NewGuid(), "New Name");

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
