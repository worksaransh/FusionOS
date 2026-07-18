using FusionOS.Modules.Sales.Domain.Discounts;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Discounts;

public class DiscountRuleTests
{
    [Fact]
    public void Create_WithValidData_StartsActive()
    {
        var rule = DiscountRule.Create(Guid.NewGuid(), Guid.NewGuid(), 50m, 10m);

        rule.IsActive.Should().BeTrue();
        rule.MinQuantity.Should().Be(50m);
        rule.DiscountPercentage.Should().Be(10m);
        rule.DomainEvents.Should().ContainSingle(e => e is Events.DiscountRuleCreated);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Create_WithNonPositiveMinQuantity_Throws(decimal minQuantity)
    {
        var act = () => DiscountRule.Create(Guid.NewGuid(), Guid.NewGuid(), minQuantity, 10m);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Create_WithInvalidDiscountPercentage_Throws(decimal discountPercentage)
    {
        var act = () => DiscountRule.Create(Guid.NewGuid(), Guid.NewGuid(), 10m, discountPercentage);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_MarksInactive()
    {
        var rule = DiscountRule.Create(Guid.NewGuid(), Guid.NewGuid(), 50m, 10m);

        rule.Deactivate();

        rule.IsActive.Should().BeFalse();
    }
}
