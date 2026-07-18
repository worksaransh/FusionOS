using FluentAssertions;
using FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;
using FusionOS.Modules.Finance.Application.ExchangeRates.Queries.GetExchangeRateById;
using FusionOS.Modules.Finance.Domain.ExchangeRates;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.ExchangeRates;

public class GetExchangeRateByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenExchangeRateExists_ReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var exchangeRate = ExchangeRate.Create(companyId, "USD", "EUR", 0.92m, DateTimeOffset.UtcNow);
        var repository = Substitute.For<IExchangeRateRepository>();
        repository.GetByIdAsync(companyId, exchangeRate.Id, Arg.Any<CancellationToken>()).Returns(exchangeRate);
        var handler = new GetExchangeRateByIdQueryHandler(repository);

        var result = await handler.Handle(new GetExchangeRateByIdQuery(companyId, exchangeRate.Id), CancellationToken.None);

        result.FromCurrencyCode.Should().Be("USD");
        result.ToCurrencyCode.Should().Be("EUR");
    }

    [Fact]
    public async Task Handle_WhenExchangeRateDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var exchangeRateId = Guid.NewGuid();
        var repository = Substitute.For<IExchangeRateRepository>();
        repository.GetByIdAsync(companyId, exchangeRateId, Arg.Any<CancellationToken>()).Returns((ExchangeRate?)null);
        var handler = new GetExchangeRateByIdQueryHandler(repository);

        var act = () => handler.Handle(new GetExchangeRateByIdQuery(companyId, exchangeRateId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
