using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;

namespace FusionOS.Modules.Finance.Application.ExchangeRates.Commands.DeactivateExchangeRate;

/// <summary>Soft-deactivate only — never a real delete (04_DATABASE_GUIDELINES.md), same convention as every other M8 sub-slice's Deactivate command. A deactivated rate is simply skipped by GetLatestRateAsync's lookup (see IExchangeRateRepository), it is not removed from history.</summary>
public sealed record DeactivateExchangeRateCommand(Guid CompanyId, Guid ExchangeRateId)
    : ICommand<ExchangeRateDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.exchange-rate.deactivate" };
    public string EntityType => nameof(Domain.ExchangeRates.ExchangeRate);
    public Guid EntityId => ExchangeRateId;
    public string Action => "Deactivated";
}
