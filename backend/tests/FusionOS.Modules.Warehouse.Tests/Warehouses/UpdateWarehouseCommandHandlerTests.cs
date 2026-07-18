using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Warehouses.Commands.UpdateWarehouse;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FluentAssertions;
using NSubstitute;
using Xunit;
using WarehouseEntity = FusionOS.Modules.Warehouse.Domain.Warehouses.Warehouse;

namespace FusionOS.Modules.Warehouse.Tests.Warehouses;

public class UpdateWarehouseCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenWarehouseBelongsToCompany_UpdatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var warehouse = WarehouseEntity.Create(companyId, null, "Old Name", "WH-01", null);
        var repository = Substitute.For<IWarehouseRepository>();
        repository.GetByIdAsync(warehouse.Id, Arg.Any<CancellationToken>()).Returns(warehouse);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateWarehouseCommandHandler(repository, unitOfWork);
        var command = new UpdateWarehouseCommand(companyId, warehouse.Id, null, "New Name", "123 Dock St");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("New Name");
        result.Address.Should().Be("123 Dock St");
        result.Code.Should().Be("WH-01");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWarehouseBelongsToDifferentCompany_ThrowsValidationException()
    {
        var warehouse = WarehouseEntity.Create(Guid.NewGuid(), null, "Old Name", "WH-01", null);
        var repository = Substitute.For<IWarehouseRepository>();
        repository.GetByIdAsync(warehouse.Id, Arg.Any<CancellationToken>()).Returns(warehouse);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateWarehouseCommandHandler(repository, unitOfWork);
        var command = new UpdateWarehouseCommand(Guid.NewGuid(), warehouse.Id, null, "New Name", null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenWarehouseDoesNotExist_ThrowsValidationException()
    {
        var repository = Substitute.For<IWarehouseRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((WarehouseEntity?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateWarehouseCommandHandler(repository, unitOfWork);
        var command = new UpdateWarehouseCommand(Guid.NewGuid(), Guid.NewGuid(), null, "New Name", null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
