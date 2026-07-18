using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Queries.ListProducts;
using FusionOS.Modules.Inventory.Domain.Products;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Products;

public class ListProductsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedProductsForTheCompany()
    {
        var companyId = Guid.NewGuid();
        var products = new[] { Product.Create(companyId, "SKU-1", "Widget", "PCS") };
        var repository = Substitute.For<IProductRepository>();
        repository.ListAsync(companyId, null, 1, 25, Arg.Any<CancellationToken>()).Returns(products);
        repository.CountAsync(companyId, null, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListProductsQueryHandler(repository);

        var result = await handler.Handle(new ListProductsQuery(companyId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(p => p.Sku == "SKU-1");
    }

    [Fact]
    public async Task Handle_PassesSearchTermThroughToTheRepository()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IProductRepository>();
        repository.ListAsync(companyId, "widget", 1, 25, Arg.Any<CancellationToken>()).Returns(Array.Empty<Product>());
        repository.CountAsync(companyId, "widget", Arg.Any<CancellationToken>()).Returns(0);
        var handler = new ListProductsQueryHandler(repository);

        await handler.Handle(new ListProductsQuery(companyId, "widget"), CancellationToken.None);

        await repository.Received(1).ListAsync(companyId, "widget", 1, 25, Arg.Any<CancellationToken>());
    }
}
