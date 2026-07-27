using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Application.SerialUnits.Commands.RegisterSerialUnit;
using FusionOS.Modules.Inventory.Application.SerialUnits.Commands.UpdateSerialUnitStatus;
using FusionOS.Modules.Inventory.Application.SerialUnits.Contracts;
using FusionOS.Modules.Inventory.Domain.Products;
using FusionOS.Modules.Inventory.Domain.SerialUnits;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.SerialUnits;

public class SerialUnitCommandHandlerTests
{
    [Fact]
    public async Task RegisterSerialUnit_WhenProductExists_PersistsUnit()
    {
        var companyId = Guid.NewGuid();
        var product = Product.Create(companyId, "WIDGET", "Widget", "PCS");
        var productRepository = Substitute.For<IProductRepository>();
        productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        var repository = Substitute.For<ISerialUnitRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RegisterSerialUnitCommandHandler(repository, productRepository, unitOfWork);

        var result = await handler.Handle(new RegisterSerialUnitCommand(companyId, product.Id, "SN-001"), CancellationToken.None);

        result.Status.Should().Be("InStock");
        await repository.Received(1).AddAsync(Arg.Any<SerialUnit>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterSerialUnit_WhenProductDoesNotExist_ThrowsValidationException()
    {
        var productRepository = Substitute.For<IProductRepository>();
        productRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);
        var handler = new RegisterSerialUnitCommandHandler(Substitute.For<ISerialUnitRepository>(), productRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new RegisterSerialUnitCommand(Guid.NewGuid(), Guid.NewGuid(), "SN-001"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task RegisterSerialUnit_WhenSerialNumberAlreadyExists_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var product = Product.Create(companyId, "WIDGET", "Widget", "PCS");
        var productRepository = Substitute.For<IProductRepository>();
        productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        var repository = Substitute.For<ISerialUnitRepository>();
        repository.SerialNumberExistsAsync(companyId, product.Id, "SN-001", Arg.Any<CancellationToken>()).Returns(true);
        var handler = new RegisterSerialUnitCommandHandler(repository, productRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new RegisterSerialUnitCommand(companyId, product.Id, "SN-001"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateSerialUnitStatus_ToReserved_Transitions()
    {
        var companyId = Guid.NewGuid();
        var unit = SerialUnit.Create(companyId, Guid.NewGuid(), "SN-001");
        var repository = Substitute.For<ISerialUnitRepository>();
        repository.GetByIdAsync(companyId, unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        var handler = new UpdateSerialUnitStatusCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new UpdateSerialUnitStatusCommand(companyId, unit.Id, SerialUnitStatus.Reserved), CancellationToken.None);

        result.Status.Should().Be("Reserved");
    }

    [Fact]
    public async Task UpdateSerialUnitStatus_ToSoldTwice_ThrowsInvalidOperation()
    {
        var companyId = Guid.NewGuid();
        var unit = SerialUnit.Create(companyId, Guid.NewGuid(), "SN-001");
        unit.MarkSold();
        var repository = Substitute.For<ISerialUnitRepository>();
        repository.GetByIdAsync(companyId, unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        var handler = new UpdateSerialUnitStatusCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new UpdateSerialUnitStatusCommand(companyId, unit.Id, SerialUnitStatus.Sold), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task UpdateSerialUnitStatus_WhenMissing_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<ISerialUnitRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((SerialUnit?)null);
        var handler = new UpdateSerialUnitStatusCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new UpdateSerialUnitStatusCommand(companyId, Guid.NewGuid(), SerialUnitStatus.Reserved), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
