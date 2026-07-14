using FusionOS.Modules.Warehouse.Application.Warehouses.Commands.CreateWarehouse;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FluentAssertions;
using NSubstitute;
using Xunit;
using WarehouseEntity = FusionOS.Modules.Warehouse.Domain.Warehouses.Warehouse;

namespace FusionOS.Modules.Warehouse.Tests.Warehouses;

public class CreateWarehouseCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCodeIsUnique_PersistsWarehouse()
    {
        var repository = Substitute.For<IWarehouseRepository>();
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateWarehouseCommandHandler(repository, unitOfWork);
        var command = new CreateWarehouseCommand(Guid.NewGuid(), null, "Main DC", "WH-01", null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Code.Should().Be("WH-01");
        await repository.Received(1).AddAsync(Arg.Any<WarehouseEntity>(), Arg.Any<CancellationToken>());
    }
}
