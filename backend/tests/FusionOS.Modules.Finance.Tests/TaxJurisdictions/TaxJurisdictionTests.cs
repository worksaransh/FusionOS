using FluentAssertions;
using FusionOS.Modules.Finance.Domain.TaxJurisdictions;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.TaxJurisdictions;

public class TaxJurisdictionTests
{
    [Fact]
    public void Create_NormalizesCode_ToUppercaseTrimmed()
    {
        var jurisdiction = TaxJurisdiction.Create(Guid.NewGuid(), "  in-ka  ", "Karnataka, India");

        jurisdiction.Code.Should().Be("IN-KA");
        jurisdiction.Name.Should().Be("Karnataka, India");
        jurisdiction.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyCode_Throws(string invalidCode)
    {
        var act = () => TaxJurisdiction.Create(Guid.NewGuid(), invalidCode, "Karnataka, India");

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyName_Throws(string invalidName)
    {
        var act = () => TaxJurisdiction.Create(Guid.NewGuid(), "IN-KA", invalidName);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateDetails_WithValidName_UpdatesName()
    {
        var jurisdiction = TaxJurisdiction.Create(Guid.NewGuid(), "IN-KA", "Karnataka, India");

        jurisdiction.UpdateDetails("Karnataka State, India");

        jurisdiction.Name.Should().Be("Karnataka State, India");
    }

    [Fact]
    public void UpdateDetails_WithEmptyName_Throws()
    {
        var jurisdiction = TaxJurisdiction.Create(Guid.NewGuid(), "IN-KA", "Karnataka, India");

        var act = () => jurisdiction.UpdateDetails(" ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var jurisdiction = TaxJurisdiction.Create(Guid.NewGuid(), "IN-KA", "Karnataka, India");

        jurisdiction.Deactivate();

        jurisdiction.IsActive.Should().BeFalse();
    }
}
