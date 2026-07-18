using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;

namespace FusionOS.Modules.Finance.Application.ExchangeRates.Queries.ConvertAmount;

/// <summary>
/// The one piece of actual "convert an amount" behavior this slice ships —
/// a pure, on-demand lookup-and-multiply, not a converter wired onto any
/// posted transaction (see ExchangeRate.cs's class doc comment for the
/// scope line). Gated by the same finance.exchange-rate.read permission as
/// GetExchangeRateByIdQuery/ListExchangeRatesQuery — this reads rate data,
/// it doesn't create/change any row, so it doesn't need its own permission
/// code.
/// </summary>
public sealed record ConvertAmountQuery(Guid CompanyId, string FromCurrencyCode, string ToCurrencyCode, decimal Amount)
    : IQuery<ConversionResultDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.exchange-rate.read" };
}
