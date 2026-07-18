using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Warehouses.Commands.DeactivateWarehouse;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FluentAssertions;
using NSubstitute;
using Xunit;
using WarehouseEntity = FusionOS.Modules.Warehouse.Domain.Warehouses.Warehouse;

namespace FusionOS.Modules.Warehouse.Tests.Warehouses;

public class DeactivateWarehouseCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenWarehouseBelongsToCompany_DeactivatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var warehouse = WarehouseEntity.Create(companyId, null, "Main DC", "WH-01", null);
        var repository = Substitute.For<IWarehouseRepository>();
        repository.GetByIdAsync(warehouse.Id, Arg.Any<CancellationToken>()).Returns(warehouse);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateWarehouseCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateWarehouseCommand(companyId, warehouse.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWarehouseBelongsToDifferentCompany_ThrowsValidationException()
    {
        var warehouse = WarehouseEntity.Create(Guid.NewGuid(), null, "Main DC", "WH-01", null);
        var repository = Substitute.For<IWarehouseRepository>();
        repository.GetByIdAsync(warehouse.Id, Arg.Any<CancellationToken>()).Returns(warehouse);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateWarehouseCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new DeactivateWarehouseCommand(Guid.NewGuid(), warehouse.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenWarehouseDoesNotExist_ThrowsValidationException()
    {
        var repository = Substitute.For<IWarehouseRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((WarehouseEntity?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateWarehouseCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new DeactivateWarehouseCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
