namespace FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;

/// <summary>Mirrors ITaxRateRepository's shape (GetById/AddAsync/ListAsync/CountAsync), plus RateExistsAsync for the (CompanyId, From, To, EffectiveDate) uniqueness check CreateExchangeRateCommandHandler/UpdateExchangeRateCommandHandler run before insert/update, and GetLatestRateAsync for ConvertAmountQueryHandler's lookup.</summary>
public interface IExchangeRateRepository
{
    Task<Domain.ExchangeRates.ExchangeRate?> GetByIdAsync(Guid companyId, Guid exchangeRateId, CancellationToken cancellationToken = default);

    /// <summary>The active rate for this currency pair with the most recent EffectiveDate that is on or before today (UTC), or null if none exists yet.</summary>
    Task<Domain.ExchangeRates.ExchangeRate?> GetLatestRateAsync(Guid companyId, string fromCurrencyCode, string toCurrencyCode, CancellationToken cancellationToken = default);

    /// <summary>True if a rate row already exists for this exact (CompanyId, From, To, EffectiveDate) tuple — the unique index ExchangeRateConfiguration enforces. excludeExchangeRateId lets UpdateExchangeRateCommandHandler check without tripping over the row it's updating.</summary>
    Task<bool> RateExistsAsync(Guid companyId, string fromCurrencyCode, string toCurrencyCode, DateTimeOffset effectiveDate, Guid? excludeExchangeRateId = null, CancellationToken cancellationToken = default);

    Task AddAsync(Domain.ExchangeRates.ExchangeRate exchangeRate, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Domain.ExchangeRates.ExchangeRate>> ListAsync(Guid companyId, string? fromCurrencyCode, string? toCurrencyCode, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountAsync(Guid companyId, string? fromCurrencyCode, string? toCurrencyCode, CancellationToken cancellationToken = default);
}
