using FluentAssertions;
using FusionOS.Modules.Finance.Domain.ExchangeRates;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.ExchangeRates;

public class ExchangeRateTests
{
    [Fact]
    public void Create_NormalizesCurrencyCodes_ToUppercaseTrimmed()
    {
        var exchangeRate = ExchangeRate.Create(Guid.NewGuid(), "  usd  ", " eur ", 0.92m, DateTimeOffset.UtcNow);

        exchangeRate.FromCurrencyCode.Should().Be("USD");
        exchangeRate.ToCurrencyCode.Should().Be("EUR");
        exchangeRate.Rate.Should().Be(0.92m);
        exchangeRate.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("US1")]
    public void Create_WithInvalidFromCurrencyCode_Throws(string invalidCode)
    {
        var act = () => ExchangeRate.Create(Guid.NewGuid(), invalidCode, "EUR", 0.92m, DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("EU")]
    [InlineData("EURR")]
    public void Create_WithInvalidToCurrencyCode_Throws(string invalidCode)
    {
        var act = () => ExchangeRate.Create(Guid.NewGuid(), "USD", invalidCode, 0.92m, DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithSameFromAndToCurrencyCode_Throws()
    {
        var act = () => ExchangeRate.Create(Guid.NewGuid(), "USD", "usd", 1m, DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1.5)]
    public void Create_WithNonPositiveRate_Throws(decimal invalidRate)
    {
        var act = () => ExchangeRate.Create(Guid.NewGuid(), "USD", "EUR", invalidRate, DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateRate_WithValidData_UpdatesRateAndEffectiveDate()
    {
        var exchangeRate = ExchangeRate.Create(Guid.NewGuid(), "USD", "EUR", 0.92m, DateTimeOffset.UtcNow);
        var newDate = DateTimeOffset.UtcNow.AddDays(1);

        exchangeRate.UpdateRate(0.93m, newDate);

        exchangeRate.Rate.Should().Be(0.93m);
        exchangeRate.EffectiveDate.Should().Be(newDate);
        exchangeRate.FromCurrencyCode.Should().Be("USD");
        exchangeRate.ToCurrencyCode.Should().Be("EUR");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void UpdateRate_WithNonPositiveRate_Throws(decimal invalidRate)
    {
        var exchangeRate = ExchangeRate.Create(Guid.NewGuid(), "USD", "EUR", 0.92m, DateTimeOffset.UtcNow);

        var act = () => exchangeRate.UpdateRate(invalidRate, DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var exchangeRate = ExchangeRate.Create(Guid.NewGuid(), "USD", "EUR", 0.92m, DateTimeOffset.UtcNow);

        exchangeRate.Deactivate();

        exchangeRate.IsActive.Should().BeFalse();
    }
}
