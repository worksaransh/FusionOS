using FusionOS.BuildingBlocks.Application.Abstractions;

namespace FusionOS.Modules.Finance.Application.ExchangeRates.Commands.RevalueForeignCurrencyBalances;

public sealed record RevalueForeignCurrencyBalancesResultDto(
    Guid CompanyId,
    string CurrencyCode,
    decimal RevaluedAmount,
    decimal GainLossAmount,
    int AccountsProcessed,
    DateTimeOffset ProcessedAt);

public sealed record RevalueForeignCurrencyBalancesCommand(
    Guid CompanyId,
    string CurrencyCode,
    decimal NewExchangeRate,
    DateTimeOffset EffectiveDate,
    string BaseCurrencyCode,
    Guid GainLossAccountId)
    : ICommand<RevalueForeignCurrencyBalancesResultDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.exchange-rate.revalue" };
    public string EntityType => "ForeignCurrencyRevaluation";
    public Guid EntityId { get; init; } = Guid.NewGuid();
    public string Action => "Revalued";
}
