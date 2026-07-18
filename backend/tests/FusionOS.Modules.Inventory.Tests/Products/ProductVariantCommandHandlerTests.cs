using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Inventory.Application.Products.Commands.AddProductVariant;
using FusionOS.Modules.Inventory.Application.Products.Commands.DeactivateProductVariant;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Products;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Products;

public class ProductVariantCommandHandlerTests
{
    [Fact]
    public async Task AddProductVariant_WhenProductBelongsToCompany_AddsVariantAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var product = Product.Create(companyId, "TSHIRT", "T-Shirt", "PCS");
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AddProductVariantCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new AddProductVariantCommand(companyId, product.Id, "TSHIRT-RED-M", "Color: Red, Size: M"),
            CancellationToken.None);

        result.Variants.Should().ContainSingle(v => v.VariantSku == "TSHIRT-RED-M" && v.IsActive);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddProductVariant_WhenProductBelongsToDifferentCompany_ThrowsValidationException()
    {
        var product = Product.Create(Guid.NewGuid(), "TSHIRT", "T-Shirt", "PCS");
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        var handler = new AddProductVariantCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(
            new AddProductVariantCommand(Guid.NewGuid(), product.Id, "TSHIRT-RED-M", "Color: Red, Size: M"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task DeactivateProductVariant_WhenExists_DeactivatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var product = Product.Create(companyId, "TSHIRT", "T-Shirt", "PCS");
        product.AddVariant("TSHIRT-RED-M", "Color: Red, Size: M");
        var variantId = product.Variants[0].Id;
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateProductVariantCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateProductVariantCommand(companyId, product.Id, variantId), CancellationToken.None);

        result.Variants.Should().ContainSingle(v => v.Id == variantId && !v.IsActive);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateProductVariant_WhenProductDoesNotExist_ThrowsValidationException()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);
        var handler = new DeactivateProductVariantCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new DeactivateProductVariantCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
