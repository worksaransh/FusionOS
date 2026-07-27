using FluentAssertions;
using FusionOS.Modules.Inventory.Domain.Attributes;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Attributes;

public class ProductVariantAttributeValueTests
{
    [Fact]
    public void Create_WithValidIds_SetsAllFields()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var definitionId = Guid.NewGuid();
        var valueId = Guid.NewGuid();

        var assignment = ProductVariantAttributeValue.Create(companyId, productId, variantId, definitionId, valueId);

        assignment.CompanyId.Should().Be(companyId);
        assignment.ProductId.Should().Be(productId);
        assignment.VariantId.Should().Be(variantId);
        assignment.AttributeDefinitionId.Should().Be(definitionId);
        assignment.AttributeValueId.Should().Be(valueId);
    }

    [Fact]
    public void Create_WithEmptyProductId_Throws()
    {
        var act = () => ProductVariantAttributeValue.Create(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyVariantId_Throws()
    {
        var act = () => ProductVariantAttributeValue.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyAttributeDefinitionId_Throws()
    {
        var act = () => ProductVariantAttributeValue.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyAttributeValueId_Throws()
    {
        var act = () => ProductVariantAttributeValue.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }
}
