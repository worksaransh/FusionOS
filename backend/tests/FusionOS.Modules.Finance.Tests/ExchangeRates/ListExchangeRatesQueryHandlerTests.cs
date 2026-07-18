using FluentAssertions;
using FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;
using FusionOS.Modules.Finance.Application.ExchangeRates.Queries.ListExchangeRates;
using FusionOS.Modules.Finance.Domain.ExchangeRates;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.ExchangeRates;

public class ListExchangeRatesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedExchangeRatesForTheCompany()
    {
        var companyId = Guid.NewGuid();
        var exchangeRates = new[] { ExchangeRate.Create(companyId, "USD", "EUR", 0.92m, DateTimeOffset.UtcNow) };
        var repository = Substitute.For<IExchangeRateRepository>();
        repository.ListAsync(companyId, null, null, 1, 25, Arg.Any<CancellationToken>()).Returns(exchangeRates);
        repository.CountAsync(companyId, null, null, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListExchangeRatesQueryHandler(repository);

        var result = await handler.Handle(new ListExchangeRatesQuery(companyId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(r => r.FromCurrencyCode == "USD" && r.ToCurrencyCode == "EUR");
    }
}
