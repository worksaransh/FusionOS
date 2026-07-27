using FluentAssertions;
using FusionOS.Modules.Inventory.Domain.Attributes;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Attributes;

public class AttributeDefinitionTests
{
    [Fact]
    public void Create_WithValidName_SetsNameAndIsActive()
    {
        var definition = AttributeDefinition.Create(Guid.NewGuid(), "  Color  ");

        definition.Name.Should().Be("Color");
        definition.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyName_Throws(string invalidName)
    {
        var act = () => AttributeDefinition.Create(Guid.NewGuid(), invalidName);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateDetails_WithValidName_UpdatesName()
    {
        var definition = AttributeDefinition.Create(Guid.NewGuid(), "Color");

        definition.UpdateDetails("Colour");

        definition.Name.Should().Be("Colour");
    }

    [Fact]
    public void UpdateDetails_WithEmptyName_Throws()
    {
        var definition = AttributeDefinition.Create(Guid.NewGuid(), "Color");

        var act = () => definition.UpdateDetails(" ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var definition = AttributeDefinition.Create(Guid.NewGuid(), "Color");

        definition.Deactivate();

        definition.IsActive.Should().BeFalse();
    }
}
