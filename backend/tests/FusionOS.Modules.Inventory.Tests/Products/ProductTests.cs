using FusionOS.Modules.Inventory.Domain.Products;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Products;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_NormalizesSkuAndUnit()
    {
        var product = Product.Create(Guid.NewGuid(), " abc-123 ", "Steel Bolt 10mm", " pcs ");

        product.Sku.Should().Be("ABC-123");
        product.UnitOfMeasure.Should().Be("PCS");
        product.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithoutSku_Throws(string invalidSku)
    {
        var act = () => Product.Create(Guid.NewGuid(), invalidSku, "Name", "PCS");

        act.Should().Throw<ArgumentException>();
    }
}
