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

    // M9-remaining e: Multi-UOM
    [Fact]
    public void AddUnitOfMeasureConversion_WithValidData_AddsNormalizedConversion()
    {
        var product = Product.Create(Guid.NewGuid(), "SKU-1", "Widget", "PCS");

        product.AddUnitOfMeasureConversion(" box ", 12);

        product.UnitOfMeasureConversions.Should().ContainSingle(c => c.AlternateUnitOfMeasure == "BOX" && c.ConversionFactor == 12);
    }

    [Fact]
    public void AddUnitOfMeasureConversion_SameAsBaseUnit_Throws()
    {
        var product = Product.Create(Guid.NewGuid(), "SKU-1", "Widget", "PCS");

        var act = () => product.AddUnitOfMeasureConversion("pcs", 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddUnitOfMeasureConversion_WhenAlternateAlreadyExists_ReplacesExisting()
    {
        var product = Product.Create(Guid.NewGuid(), "SKU-1", "Widget", "PCS");
        product.AddUnitOfMeasureConversion("BOX", 12);

        product.AddUnitOfMeasureConversion("BOX", 24);

        product.UnitOfMeasureConversions.Should().ContainSingle(c => c.AlternateUnitOfMeasure == "BOX" && c.ConversionFactor == 24);
    }

    [Fact]
    public void RemoveUnitOfMeasureConversion_WhenExists_RemovesIt()
    {
        var product = Product.Create(Guid.NewGuid(), "SKU-1", "Widget", "PCS");
        product.AddUnitOfMeasureConversion("BOX", 12);

        product.RemoveUnitOfMeasureConversion("box");

        product.UnitOfMeasureConversions.Should().BeEmpty();
    }

    [Fact]
    public void RemoveUnitOfMeasureConversion_WhenNotFound_Throws()
    {
        var product = Product.Create(Guid.NewGuid(), "SKU-1", "Widget", "PCS");

        var act = () => product.RemoveUnitOfMeasureConversion("BOX");

        act.Should().Throw<ArgumentException>();
    }
}
