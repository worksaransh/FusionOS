using FusionOS.Modules.Core.Domain.Settings;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Settings;

public class CompanySettingsTests
{
    [Fact]
    public void CreateDefault_WithValidCompanyId_SetsSensibleDefaults()
    {
        var companyId = Guid.NewGuid();

        var settings = CompanySettings.CreateDefault(companyId);

        settings.CompanyId.Should().Be(companyId);
        settings.DefaultCurrency.Should().Be("USD");
        settings.DefaultPageSize.Should().Be(25);
        settings.DisplayName.Should().BeNull();
        settings.LogoUrl.Should().BeNull();
    }

    [Fact]
    public void CreateDefault_WithEmptyCompanyId_Throws()
    {
        var act = () => CompanySettings.CreateDefault(Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateSettings_WithValidData_UpdatesAllFields()
    {
        var settings = CompanySettings.CreateDefault(Guid.NewGuid());

        settings.UpdateSettings("inr", 50, "  Acme Trading  ", "  https://example.com/logo.png  ");

        settings.DefaultCurrency.Should().Be("INR");
        settings.DefaultPageSize.Should().Be(50);
        settings.DisplayName.Should().Be("Acme Trading");
        settings.LogoUrl.Should().Be("https://example.com/logo.png");
    }

    [Fact]
    public void UpdateSettings_WithBlankDisplayNameAndLogoUrl_ClearsThemToNull()
    {
        var settings = CompanySettings.CreateDefault(Guid.NewGuid());
        settings.UpdateSettings("USD", 25, "Something", "https://example.com/x.png");

        settings.UpdateSettings("USD", 25, "   ", "   ");

        settings.DisplayName.Should().BeNull();
        settings.LogoUrl.Should().BeNull();
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDX")]
    [InlineData("")]
    public void UpdateSettings_WithInvalidCurrencyLength_Throws(string currency)
    {
        var settings = CompanySettings.CreateDefault(Guid.NewGuid());

        var act = () => settings.UpdateSettings(currency, 25, null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(201)]
    [InlineData(-5)]
    public void UpdateSettings_WithPageSizeOutOfRange_Throws(int pageSize)
    {
        var settings = CompanySettings.CreateDefault(Guid.NewGuid());

        var act = () => settings.UpdateSettings("USD", pageSize, null, null);

        act.Should().Throw<ArgumentException>();
    }
}
