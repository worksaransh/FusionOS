using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;

namespace FusionOS.Modules.Finance.Application.ExchangeRates.Commands.CreateExchangeRate;

public sealed record CreateExchangeRateCommand(Guid CompanyId, string FromCurrencyCode, string ToCurrencyCode, decimal Rate, DateTimeOffset EffectiveDate)
    : ICommand<ExchangeRateDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.exchange-rate.create" };
    public string EntityType => nameof(Domain.ExchangeRates.ExchangeRate);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
