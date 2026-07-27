using FluentAssertions;
using FusionOS.Modules.Inventory.Domain.Attributes;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Attributes;

public class AttributeValueTests
{
    [Fact]
    public void Create_WithValidValue_SetsValueAndIsActive()
    {
        var definitionId = Guid.NewGuid();

        var value = AttributeValue.Create(Guid.NewGuid(), definitionId, "  Red  ");

        value.AttributeDefinitionId.Should().Be(definitionId);
        value.Value.Should().Be("Red");
        value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyAttributeDefinitionId_Throws()
    {
        var act = () => AttributeValue.Create(Guid.NewGuid(), Guid.Empty, "Red");

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyValue_Throws(string invalidValue)
    {
        var act = () => AttributeValue.Create(Guid.NewGuid(), Guid.NewGuid(), invalidValue);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var value = AttributeValue.Create(Guid.NewGuid(), Guid.NewGuid(), "Red");

        value.Deactivate();

        value.IsActive.Should().BeFalse();
    }
}
