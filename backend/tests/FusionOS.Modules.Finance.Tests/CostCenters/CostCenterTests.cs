using FluentAssertions;
using FusionOS.Modules.Finance.Domain.CostCenters;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.CostCenters;

public class CostCenterTests
{
    [Fact]
    public void Create_NormalizesCode_ToUppercaseTrimmed()
    {
        var costCenter = CostCenter.Create(Guid.NewGuid(), "  cc-100  ", "Manufacturing");

        costCenter.Code.Should().Be("CC-100");
        costCenter.Name.Should().Be("Manufacturing");
        costCenter.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyCode_Throws(string invalidCode)
    {
        var act = () => CostCenter.Create(Guid.NewGuid(), invalidCode, "Manufacturing");

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyName_Throws(string invalidName)
    {
        var act = () => CostCenter.Create(Guid.NewGuid(), "CC-100", invalidName);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateDetails_WithValidName_UpdatesName()
    {
        var costCenter = CostCenter.Create(Guid.NewGuid(), "CC-100", "Manufacturing");

        costCenter.UpdateDetails("Manufacturing (East)");

        costCenter.Name.Should().Be("Manufacturing (East)");
    }

    [Fact]
    public void UpdateDetails_WithEmptyName_Throws()
    {
        var costCenter = CostCenter.Create(Guid.NewGuid(), "CC-100", "Manufacturing");

        var act = () => costCenter.UpdateDetails(" ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var costCenter = CostCenter.Create(Guid.NewGuid(), "CC-100", "Manufacturing");

        costCenter.Deactivate();

        costCenter.IsActive.Should().BeFalse();
    }
}
