using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Inventory.Application.Products.Commands.UpdateProduct;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Products;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Products;

public class UpdateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenProductBelongsToCompany_UpdatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var product = Product.Create(companyId, "SKU-1", "Old Name", "PCS");
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateProductCommandHandler(repository, unitOfWork);
        var command = new UpdateProductCommand(companyId, product.Id, "New Name", "KG", "Updated description");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("New Name");
        result.UnitOfMeasure.Should().Be("KG");
        result.Sku.Should().Be("SKU-1");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductBelongsToDifferentCompany_ThrowsValidationException()
    {
        var product = Product.Create(Guid.NewGuid(), "SKU-1", "Old Name", "PCS");
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateProductCommandHandler(repository, unitOfWork);
        var command = new UpdateProductCommand(Guid.NewGuid(), product.Id, "New Name", "KG", null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ThrowsValidationException()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateProductCommandHandler(repository, unitOfWork);
        var command = new UpdateProductCommand(Guid.NewGuid(), Guid.NewGuid(), "New Name", "KG", null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
