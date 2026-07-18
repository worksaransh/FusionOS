using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;

namespace FusionOS.Modules.Finance.Application.ExchangeRates.Queries.ListExchangeRates;

public sealed record ListExchangeRatesQuery(Guid CompanyId, string? FromCurrencyCode = null, string? ToCurrencyCode = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<ExchangeRateDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.exchange-rate.read" };
}
