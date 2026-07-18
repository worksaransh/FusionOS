using FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;
using FusionOS.Modules.Finance.Domain.ExchangeRates;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class ExchangeRateRepository : IExchangeRateRepository
{
    private readonly FinanceDbContext _context;

    public ExchangeRateRepository(FinanceDbContext context) => _context = context;

    public Task<ExchangeRate?> GetByIdAsync(Guid companyId, Guid exchangeRateId, CancellationToken cancellationToken = default) =>
        _context.ExchangeRates.FirstOrDefaultAsync(r => r.CompanyId == companyId && r.Id == exchangeRateId, cancellationToken);

    public async Task<ExchangeRate?> GetLatestRateAsync(Guid companyId, string fromCurrencyCode, string toCurrencyCode, CancellationToken cancellationToken = default)
    {
        var from = fromCurrencyCode.Trim().ToUpper();
        var to = toCurrencyCode.Trim().ToUpper();
        var today = DateTimeOffset.UtcNow;

        return await _context.ExchangeRates
            .Where(r => r.CompanyId == companyId && r.IsActive && r.FromCurrencyCode == from && r.ToCurrencyCode == to && r.EffectiveDate <= today)
            .OrderByDescending(r => r.EffectiveDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> RateExistsAsync(Guid companyId, string fromCurrencyCode, string toCurrencyCode, DateTimeOffset effectiveDate, Guid? excludeExchangeRateId = null, CancellationToken cancellationToken = default)
    {
        var from = fromCurrencyCode.Trim().ToUpper();
        var to = toCurrencyCode.Trim().ToUpper();

        return _context.ExchangeRates.AnyAsync(r =>
            r.CompanyId == companyId && r.FromCurrencyCode == from && r.ToCurrencyCode == to &&
            r.EffectiveDate == effectiveDate && (excludeExchangeRateId == null || r.Id != excludeExchangeRateId),
            cancellationToken);
    }

    public async Task AddAsync(ExchangeRate exchangeRate, CancellationToken cancellationToken = default) =>
        await _context.ExchangeRates.AddAsync(exchangeRate, cancellationToken);

    public async Task<IReadOnlyList<ExchangeRate>> ListAsync(Guid companyId, string? fromCurrencyCode, string? toCurrencyCode, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, fromCurrencyCode, toCurrencyCode)
            .OrderByDescending(r => r.EffectiveDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? fromCurrencyCode, string? toCurrencyCode, CancellationToken cancellationToken = default) =>
        Filtered(companyId, fromCurrencyCode, toCurrencyCode).CountAsync(cancellationToken);

    private IQueryable<ExchangeRate> Filtered(Guid companyId, string? fromCurrencyCode, string? toCurrencyCode)
    {
        var query = _context.ExchangeRates.Where(r => r.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(fromCurrencyCode))
        {
            var from = fromCurrencyCode.Trim().ToUpper();
            query = query.Where(r => r.FromCurrencyCode == from);
        }
        if (!string.IsNullOrWhiteSpace(toCurrencyCode))
        {
            var to = toCurrencyCode.Trim().ToUpper();
            query = query.Where(r => r.ToCurrencyCode == to);
        }
        return query;
    }
}
