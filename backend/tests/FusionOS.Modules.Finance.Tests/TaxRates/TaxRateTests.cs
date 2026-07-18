using FluentAssertions;
using FusionOS.Modules.Finance.Domain.TaxRates;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.TaxRates;

public class TaxRateTests
{
    [Fact]
    public void Create_NormalizesCode_ToUppercaseTrimmed()
    {
        var taxRate = TaxRate.Create(Guid.NewGuid(), Guid.NewGuid(), "  gst-standard  ", "GST 18%", 18.00m);

        taxRate.Code.Should().Be("GST-STANDARD");
        taxRate.Name.Should().Be("GST 18%");
        taxRate.Percentage.Should().Be(18.00m);
        taxRate.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyTaxJurisdictionId_Throws()
    {
        var act = () => TaxRate.Create(Guid.NewGuid(), Guid.Empty, "GST-STANDARD", "GST 18%", 18.00m);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyCode_Throws(string invalidCode)
    {
        var act = () => TaxRate.Create(Guid.NewGuid(), Guid.NewGuid(), invalidCode, "GST 18%", 18.00m);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyName_Throws(string invalidName)
    {
        var act = () => TaxRate.Create(Guid.NewGuid(), Guid.NewGuid(), "GST-STANDARD", invalidName, 18.00m);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public void Create_WithPercentageOutOfRange_Throws(decimal invalidPercentage)
    {
        var act = () => TaxRate.Create(Guid.NewGuid(), Guid.NewGuid(), "GST-STANDARD", "GST 18%", invalidPercentage);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateDetails_WithValidNameAndPercentage_Updates()
    {
        var taxRate = TaxRate.Create(Guid.NewGuid(), Guid.NewGuid(), "GST-STANDARD", "GST 18%", 18.00m);

        taxRate.UpdateDetails("GST 18% (Standard)", 18.50m);

        taxRate.Name.Should().Be("GST 18% (Standard)");
        taxRate.Percentage.Should().Be(18.50m);
    }

    [Fact]
    public void UpdateDetails_WithEmptyName_Throws()
    {
        var taxRate = TaxRate.Create(Guid.NewGuid(), Guid.NewGuid(), "GST-STANDARD", "GST 18%", 18.00m);

        var act = () => taxRate.UpdateDetails(" ", 18.00m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateDetails_WithPercentageOutOfRange_Throws()
    {
        var taxRate = TaxRate.Create(Guid.NewGuid(), Guid.NewGuid(), "GST-STANDARD", "GST 18%", 18.00m);

        var act = () => taxRate.UpdateDetails("GST 18%", 150m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var taxRate = TaxRate.Create(Guid.NewGuid(), Guid.NewGuid(), "GST-STANDARD", "GST 18%", 18.00m);

        taxRate.Deactivate();

        taxRate.IsActive.Should().BeFalse();
    }
}
