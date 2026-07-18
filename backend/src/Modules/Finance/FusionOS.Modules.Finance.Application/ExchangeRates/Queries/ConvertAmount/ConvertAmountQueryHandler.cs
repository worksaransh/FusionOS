using FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.ExchangeRates.Queries.ConvertAmount;

public sealed class ConvertAmountQueryHandler : IRequestHandler<ConvertAmountQuery, ConversionResultDto>
{
    private readonly IExchangeRateRepository _repository;

    public ConvertAmountQueryHandler(IExchangeRateRepository repository) => _repository = repository;

    public async Task<ConversionResultDto> Handle(ConvertAmountQuery request, CancellationToken cancellationToken)
    {
        var from = request.FromCurrencyCode.Trim().ToUpperInvariant();
        var to = request.ToCurrencyCode.Trim().ToUpperInvariant();

        // Converting a currency to itself is rejected at the ExchangeRate.Create
        // level (a data-entry error worth catching for master data), but a caller
        // of this pure query legitimately may not know in advance whether From/To
        // happen to match (e.g. a UI that always calls convert before displaying
        // an amount) — so this identity case is handled here rather than forcing
        // every caller to special-case it themselves, without needing any rate
        // row to exist for it.
        if (from == to)
            return new ConversionResultDto(request.Amount, from, to, request.Amount, 1m, DateTimeOffset.UtcNow);

        var rate = await _repository.GetLatestRateAsync(request.CompanyId, from, to, cancellationToken)
            ?? throw new KeyNotFoundException($"No exchange rate exists for {from}->{to} yet.");

        return new ConversionResultDto(request.Amount, from, to, request.Amount * rate.Rate, rate.Rate, rate.EffectiveDate);
    }
}
