using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;

namespace FusionOS.Modules.Finance.Application.ExchangeRates.Commands.UpdateExchangeRate;

/// <summary>Update deliberately excludes FromCurrencyCode/ToCurrencyCode — the currency pair is the immutable business key, same convention as UpdateBankAccountCommand excluding Code/LinkedAccountId. Only Rate and EffectiveDate are correctable, see ExchangeRate.UpdateRate's own doc comment.</summary>
public sealed record UpdateExchangeRateCommand(Guid CompanyId, Guid ExchangeRateId, decimal Rate, DateTimeOffset EffectiveDate)
    : ICommand<ExchangeRateDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.exchange-rate.update" };
    public string EntityType => nameof(Domain.ExchangeRates.ExchangeRate);
    public Guid EntityId => ExchangeRateId;
    public string Action => "Updated";
}
