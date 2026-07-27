using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Inventory.Application.Attributes.Commands.AssignAttributeValueToVariant;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Domain.Attributes;
using FusionOS.Modules.Inventory.Domain.Products;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Attributes;

public class AssignAttributeValueToVariantCommandHandlerTests
{
    private static (IProductVariantAttributeValueRepository assignments, IProductRepository products, IAttributeValueRepository values, IAttributeDefinitionRepository definitions, IUnitOfWork unitOfWork)
        CreateSubstitutes() =>
        (Substitute.For<IProductVariantAttributeValueRepository>(),
         Substitute.For<IProductRepository>(),
         Substitute.For<IAttributeValueRepository>(),
         Substitute.For<IAttributeDefinitionRepository>(),
         Substitute.For<IUnitOfWork>());

    [Fact]
    public async Task Handle_WhenEverythingValidAndNoExistingAssignment_CreatesAssignment()
    {
        var (assignments, products, values, definitions, unitOfWork) = CreateSubstitutes();
        var companyId = Guid.NewGuid();
        var product = Product.Create(companyId, "TSHIRT", "T-Shirt", "PCS");
        product.AddVariant("TSHIRT-RED-M", "Color: Red, Size: M");
        var variantId = product.Variants[0].Id;
        var definition = AttributeDefinition.Create(companyId, "Color");
        var value = AttributeValue.Create(companyId, definition.Id, "Red");

        products.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        products.VariantExistsAsync(product.Id, variantId, Arg.Any<CancellationToken>()).Returns(true);
        values.GetByIdAsync(companyId, value.Id, Arg.Any<CancellationToken>()).Returns(value);
        definitions.GetByIdAsync(companyId, definition.Id, Arg.Any<CancellationToken>()).Returns(definition);
        assignments.GetForVariantAndDefinitionAsync(variantId, definition.Id, Arg.Any<CancellationToken>()).Returns((ProductVariantAttributeValue?)null);

        var handler = new AssignAttributeValueToVariantCommandHandler(assignments, products, values, definitions, unitOfWork);

        var result = await handler.Handle(new AssignAttributeValueToVariantCommand(companyId, product.Id, variantId, value.Id), CancellationToken.None);

        result.AttributeDefinitionName.Should().Be("Color");
        result.AttributeValue.Should().Be("Red");
        await assignments.Received(1).AddAsync(Arg.Any<ProductVariantAttributeValue>(), Arg.Any<CancellationToken>());
        assignments.DidNotReceive().Remove(Arg.Any<ProductVariantAttributeValue>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenVariantAlreadyHasAValueForTheDefinition_ReplacesIt()
    {
        var (assignments, products, values, definitions, unitOfWork) = CreateSubstitutes();
        var companyId = Guid.NewGuid();
        var product = Product.Create(companyId, "TSHIRT", "T-Shirt", "PCS");
        product.AddVariant("TSHIRT-RED-M", "Color: Red, Size: M");
        var variantId = product.Variants[0].Id;
        var definition = AttributeDefinition.Create(companyId, "Color");
        var oldValue = AttributeValue.Create(companyId, definition.Id, "Blue");
        var newValue = AttributeValue.Create(companyId, definition.Id, "Red");
        var existingAssignment = ProductVariantAttributeValue.Create(companyId, product.Id, variantId, definition.Id, oldValue.Id);

        products.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        products.VariantExistsAsync(product.Id, variantId, Arg.Any<CancellationToken>()).Returns(true);
        values.GetByIdAsync(companyId, newValue.Id, Arg.Any<CancellationToken>()).Returns(newValue);
        definitions.GetByIdAsync(companyId, definition.Id, Arg.Any<CancellationToken>()).Returns(definition);
        assignments.GetForVariantAndDefinitionAsync(variantId, definition.Id, Arg.Any<CancellationToken>()).Returns(existingAssignment);

        var handler = new AssignAttributeValueToVariantCommandHandler(assignments, products, values, definitions, unitOfWork);

        await handler.Handle(new AssignAttributeValueToVariantCommand(companyId, product.Id, variantId, newValue.Id), CancellationToken.None);

        assignments.Received(1).Remove(existingAssignment);
        await assignments.Received(1).AddAsync(Arg.Any<ProductVariantAttributeValue>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_Throws()
    {
        var (assignments, products, values, definitions, unitOfWork) = CreateSubstitutes();
        products.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);
        var handler = new AssignAttributeValueToVariantCommandHandler(assignments, products, values, definitions, unitOfWork);

        var act = () => handler.Handle(new AssignAttributeValueToVariantCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenVariantNotFoundOnProduct_Throws()
    {
        var (assignments, products, values, definitions, unitOfWork) = CreateSubstitutes();
        var companyId = Guid.NewGuid();
        var product = Product.Create(companyId, "TSHIRT", "T-Shirt", "PCS");
        products.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        products.VariantExistsAsync(product.Id, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var handler = new AssignAttributeValueToVariantCommandHandler(assignments, products, values, definitions, unitOfWork);

        var act = () => handler.Handle(new AssignAttributeValueToVariantCommand(companyId, product.Id, Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenAttributeValueNotFound_Throws()
    {
        var (assignments, products, values, definitions, unitOfWork) = CreateSubstitutes();
        var companyId = Guid.NewGuid();
        var product = Product.Create(companyId, "TSHIRT", "T-Shirt", "PCS");
        product.AddVariant("TSHIRT-RED-M", "Color: Red, Size: M");
        var variantId = product.Variants[0].Id;
        products.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        products.VariantExistsAsync(product.Id, variantId, Arg.Any<CancellationToken>()).Returns(true);
        values.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AttributeValue?)null);
        var handler = new AssignAttributeValueToVariantCommandHandler(assignments, products, values, definitions, unitOfWork);

        var act = () => handler.Handle(new AssignAttributeValueToVariantCommand(companyId, product.Id, variantId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
