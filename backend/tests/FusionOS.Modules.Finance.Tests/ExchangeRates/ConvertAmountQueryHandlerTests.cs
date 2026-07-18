using FluentAssertions;
using FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;
using FusionOS.Modules.Finance.Application.ExchangeRates.Queries.ConvertAmount;
using FusionOS.Modules.Finance.Domain.ExchangeRates;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.ExchangeRates;

public class ConvertAmountQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenRateExists_ReturnsConvertedAmountAndRateUsed()
    {
        var companyId = Guid.NewGuid();
        var effectiveDate = DateTimeOffset.UtcNow.AddDays(-1);
        var exchangeRate = ExchangeRate.Create(companyId, "USD", "EUR", 0.92m, effectiveDate);
        var repository = Substitute.For<IExchangeRateRepository>();
        repository.GetLatestRateAsync(companyId, "USD", "EUR", Arg.Any<CancellationToken>()).Returns(exchangeRate);
        var handler = new ConvertAmountQueryHandler(repository);

        var result = await handler.Handle(new ConvertAmountQuery(companyId, "usd", "eur", 100m), CancellationToken.None);

        result.ConvertedAmount.Should().Be(92m);
        result.RateUsed.Should().Be(0.92m);
        result.EffectiveDateOfRateUsed.Should().Be(effectiveDate);
    }

    [Fact]
    public async Task Handle_WhenNoRateExists_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IExchangeRateRepository>();
        repository.GetLatestRateAsync(companyId, "USD", "GBP", Arg.Any<CancellationToken>()).Returns((ExchangeRate?)null);
        var handler = new ConvertAmountQueryHandler(repository);

        var act = () => handler.Handle(new ConvertAmountQuery(companyId, "USD", "GBP", 100m), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenFromAndToCurrencyAreTheSame_ReturnsIdentityConversionWithoutLookup()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IExchangeRateRepository>();
        var handler = new ConvertAmountQueryHandler(repository);

        var result = await handler.Handle(new ConvertAmountQuery(companyId, "USD", "usd", 100m), CancellationToken.None);

        result.ConvertedAmount.Should().Be(100m);
        result.RateUsed.Should().Be(1m);
        await repository.DidNotReceive().GetLatestRateAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
