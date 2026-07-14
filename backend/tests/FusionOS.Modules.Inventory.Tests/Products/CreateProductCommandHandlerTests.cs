using FusionOS.Modules.Inventory.Application.Products.Commands.CreateProduct;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Products;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Products;

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenSkuIsUnique_PersistsProduct()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.SkuExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateProductCommandHandler(repository, unitOfWork);
        var command = new CreateProductCommand(Guid.NewGuid(), "SKU-1", "Widget", "PCS", null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Sku.Should().Be("SKU-1");
        await repository.Received(1).AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSkuAlreadyExists_ThrowsValidationException()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.SkuExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateProductCommandHandler(repository, unitOfWork);
        var command = new CreateProductCommand(Guid.NewGuid(), "SKU-1", "Widget", "PCS", null);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FusionOS.BuildingBlocks.Application.Exceptions.ValidationException>();
    }
}
