using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Queries.GetProductById;
using FusionOS.Modules.Inventory.Domain.Products;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Products;

public class GetProductByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenProductBelongsToCompany_ReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var product = Product.Create(companyId, "SKU-1", "Widget", "PCS");
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        var handler = new GetProductByIdQueryHandler(repository);

        var result = await handler.Handle(new GetProductByIdQuery(companyId, product.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Sku.Should().Be("SKU-1");
    }

    [Fact]
    public async Task Handle_WhenProductBelongsToDifferentCompany_ReturnsNull()
    {
        var product = Product.Create(Guid.NewGuid(), "SKU-1", "Widget", "PCS");
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        var handler = new GetProductByIdQueryHandler(repository);

        var result = await handler.Handle(new GetProductByIdQuery(Guid.NewGuid(), product.Id), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ReturnsNull()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);
        var handler = new GetProductByIdQueryHandler(repository);

        var result = await handler.Handle(new GetProductByIdQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}
